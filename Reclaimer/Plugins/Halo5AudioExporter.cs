using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Halo5;
using Reclaimer.Utilities;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Reclaimer.Plugins
{
    internal static class Halo5AudioExporter
    {
        internal sealed record ExportResult(int Tags, int Banks, int Media, IReadOnlyList<string> Failures);
        private sealed record PckEntry(string Package, string Kind, uint Id, uint BlockSize, ulong Size, ulong Offset);
        private sealed record BankObject(byte Type, uint Id, byte[] Payload);
        private sealed record MediaEntry(uint Id, uint Offset, uint Size);
        private sealed record TrackClip(uint MediaId, double PlayAtMs, double BeginTrimMs, double EndTrimMs, double SourceDurationMs, double? FadeStartMs = null, double? FadeDurationMs = null);

        public static ExportResult Export(IEnumerable<IModuleItem> items, string outputFolder, string vgmstreamPath = null, string language = "English(US)")
        {
            var tags = items.OfType<ModuleItem>().Where(IsAudioTag).Distinct().ToList();
            if (tags.Count == 0)
                return new(0, 0, 0, Array.Empty<string>());

            var failures = new List<string>();
            var bankCount = 0;
            var mediaCount = 0;
            foreach (var moduleGroup in tags.GroupBy(item => item.Module.FileName, StringComparer.OrdinalIgnoreCase))
            {
                var packageFolders = FindPackageFolders(moduleGroup.Key, language);
                if (packageFolders.Count == 0)
                {
                    failures.Add($"No sound\\win package folders were found for {moduleGroup.Key}");
                    continue;
                }

                List<PckEntry> packageEntries;
                try
                {
                    packageEntries = packageFolders.SelectMany(folder => Directory.EnumerateFiles(folder, "*.pck"))
                        .SelectMany(ReadPackageEntries)
                        .ToList();
                }
                catch (Exception ex)
                {
                    failures.Add($"Unable to index Halo 5 audio packages: {ex.Message}");
                    continue;
                }

                var bankEntries = packageEntries.Where(entry => entry.Kind == "bnk").ToList();
                var soundEntries = packageEntries.Where(entry => entry.Kind == "wem")
                    .GroupBy(entry => entry.Id)
                    .ToDictionary(group => group.Key, group => group.OrderByDescending(entry => entry.Size).First());
                var banksByEvent = new Dictionary<uint, List<PckEntry>>();
                foreach (var bankEntry in bankEntries)
                {
                    foreach (var eventId in ReadBankEventIds(bankEntry))
                    {
                        if (!banksByEvent.TryGetValue(eventId, out var eventBanks))
                            banksByEvent.Add(eventId, eventBanks = new List<PckEntry>());
                        eventBanks.Add(bankEntry);
                    }
                }

                foreach (var tag in moduleGroup)
                {
                    try
                    {
                        var candidates = ReadCandidateIds(tag);
                        var matchedBanks = new List<(PckEntry Entry, List<BankObject> Objects, List<MediaEntry> Media, long DataOffset)>();
                        var matchedEntries = candidates.Where(banksByEvent.ContainsKey)
                            .SelectMany(id => banksByEvent[id])
                            .Concat(tag.ClassCode == "sbnk" ? bankEntries.Where(entry => candidates.Contains(entry.Id)) : Enumerable.Empty<PckEntry>())
                            .Distinct()
                            .ToList();
                        foreach (var entry in matchedEntries)
                        {
                            var parsed = ReadBank(entry, candidates);
                            if (parsed.Objects.Any(obj => obj.Type == 4 && candidates.Contains(obj.Id))
                                || tag.ClassCode == "sbnk" && candidates.Contains(entry.Id))
                                matchedBanks.Add((entry, parsed.Objects, parsed.Media, parsed.DataOffset));
                        }

                        if (matchedBanks.Count == 0)
                            throw new InvalidDataException("No Wwise bank contained an event or bank ID present in the tag payload.");

                        var tagFolder = Path.Combine(outputFolder, "halo5_audio", SafeRelativePath(tag.TagName));
                        Directory.CreateDirectory(tagFolder);
                        var manifest = new List<string>
                        {
                            $"tag={tag.TagName}.{tag.ClassName}",
                            $"module={tag.Module.FileName}"
                        };
                        var unconvertedMedia = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var processedDirectMedia = new HashSet<uint>();
                        var copiedBanks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        foreach (var match in matchedBanks.GroupBy(value => (value.Entry.Package, value.Entry.Offset)).Select(group => group.First()))
                        {
                            var bankPath = Path.Combine(tagFolder, $"{match.Entry.Id}.bnk");
                            CopyEntry(match.Entry, bankPath);
                            copiedBanks.Add(bankPath);
                            bankCount++;
                            manifest.Add($"bank={match.Entry.Id} package={match.Entry.Package} offset=0x{match.Entry.Offset:X} size={match.Entry.Size}");

                            var txtpPaths = new List<string>();
                            var eventIds = match.Objects.Where(obj => obj.Type == 4 && candidates.Contains(obj.Id)).Select(obj => obj.Id).ToHashSet();
                            foreach (var eventId in eventIds)
                            {
                                AppendEventGraph(manifest, eventId, match.Objects);
                                txtpPaths.AddRange(WriteTxtps(tagFolder, tag, eventId, match.Objects, manifest));
                            }

                            var embeddedMedia = match.Media.GroupBy(value => value.Id).ToDictionary(group => group.Key, group => group.First());
                            var availableMediaIds = embeddedMedia.Keys.Concat(soundEntries.Keys).ToHashSet();
                            var directMediaIds = FindReachableMedia(eventIds, match.Objects, availableMediaIds);
                            var mediaIds = txtpPaths.Count > 0 || tag.ClassCode == "sbnk" ? embeddedMedia.Keys.ToHashSet() : directMediaIds;
                            var resolvedMediaCount = mediaIds.Count;
                            manifest.Add($"selected_media={mediaIds.Count} bank_media={embeddedMedia.Count}");
                            if (txtpPaths.Count == 0 && tag.ClassCode != "sbnk")
                            {
                                mediaIds.ExceptWith(processedDirectMedia);
                                processedDirectMedia.UnionWith(mediaIds);
                                manifest.Add($"new_media={mediaIds.Count}");
                            }
                            if (txtpPaths.Count == 0 && resolvedMediaCount == 0)
                                manifest.Add("notice=This matched bank did not contribute media for the selected event graph.");

                            var mediaPaths = new List<string>();
                            foreach (var mediaId in mediaIds)
                            {
                                var mediaPath = Path.Combine(tagFolder, $"{mediaId}.wem");
                                if (soundEntries.TryGetValue(mediaId, out var fullMedia))
                                {
                                    CopyEntry(fullMedia, mediaPath);
                                    manifest.Add($"media={mediaId} source=package package={fullMedia.Package} size={fullMedia.Size}");
                                }
                                else if (embeddedMedia.TryGetValue(mediaId, out var media))
                                {
                                    CopyEmbeddedMedia(match.Entry, match.DataOffset + media.Offset, media.Size, mediaPath);
                                    manifest.Add($"media={mediaId} source=bank size={media.Size}");
                                }
                                else
                                    continue;
                                mediaPaths.Add(mediaPath);
                                mediaCount++;
                            }
                            if (File.Exists(vgmstreamPath))
                            {
                                if (txtpPaths.Count > 0)
                                {
                                    var allCompositionsConverted = true;
                                    foreach (var txtpPath in txtpPaths)
                                    {
                                        var wavPath = Path.ChangeExtension(txtpPath, ".wav");
                                        if (TryConvertToWav(vgmstreamPath, txtpPath, wavPath, manifest))
                                            manifest.Add($"wav={Path.GetFileName(wavPath)} source=composition");
                                        else
                                            allCompositionsConverted = false;
                                    }
                                    if (allCompositionsConverted)
                                        DeleteIntermediateMedia(mediaPaths, manifest);
                                }
                                else
                                {
                                    foreach (var mediaPath in mediaPaths)
                                    {
                                        var wavPath = Path.ChangeExtension(mediaPath, ".wav");
                                        if (TryConvertToWav(vgmstreamPath, mediaPath, wavPath, manifest))
                                        {
                                            manifest.Add($"wav={Path.GetFileName(wavPath)} source={Path.GetFileName(mediaPath)}");
                                            File.Delete(mediaPath);
                                            manifest.Add($"removed_intermediate={Path.GetFileName(mediaPath)}");
                                        }
                                        else
                                            unconvertedMedia.Add(mediaPath);
                                    }
                                }
                            }
                        }
                        ArchiveUnconvertedMedia(tagFolder, unconvertedMedia, manifest);
                        DeleteSourceBanks(copiedBanks, manifest);
                        File.WriteAllLines(Path.Combine(tagFolder, "manifest.txt"), manifest);
                    }
                    catch (Exception ex)
                    {
                        failures.Add($"{tag.TagName}: {ex.Message}");
                    }
                }
            }
            return new(tags.Count, bankCount, mediaCount, failures);
        }

        private static bool IsAudioTag(ModuleItem item) => item.ClassCode is "snd!" or "sbnk" or "lsnd" or "musc";

        private static HashSet<uint> ReadCandidateIds(ModuleItem item)
        {
            using var reader = item.CreateReader();
            var bytes = new byte[reader.BaseStream.Length];
            reader.BaseStream.ReadExactly(bytes);
            var values = new HashSet<uint>();
            for (var offset = 0; offset <= bytes.Length - 4; offset++)
                values.Add(BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4)));

            if (item.ClassCode == "sbnk")
                values.Add(WwiseHash(Path.GetFileName(item.TagName)));
            return values;
        }

        private static IReadOnlyList<string> FindPackageFolders(string modulePath, string language)
        {
            for (var directory = new DirectoryInfo(Path.GetDirectoryName(modulePath)); directory != null; directory = directory.Parent)
            {
                var winFolder = Path.Combine(directory.FullName, "sound", "win");
                var sfxFolder = Path.Combine(winFolder, "SFX");
                if (!Directory.Exists(sfxFolder))
                    continue;
                var folders = new List<string> { sfxFolder };
                if (!string.IsNullOrWhiteSpace(language))
                {
                    var languageFolder = Path.Combine(winFolder, language);
                    if (Directory.Exists(languageFolder))
                        folders.Add(languageFolder);
                }
                return folders;
            }
            return Array.Empty<string>();
        }

        private static IEnumerable<PckEntry> ReadPackageEntries(string path)
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "AKPK")
                throw new InvalidDataException($"{path} is not an AKPK package.");
            var headerSize = reader.ReadUInt32();
            reader.ReadUInt32();
            var languageSize = reader.ReadUInt32();
            var bankSize = reader.ReadUInt32();
            var soundSize = reader.ReadUInt32();
            var externalSize = 0u;
            if (0x10u + languageSize + bankSize + soundSize < headerSize)
                externalSize = reader.ReadUInt32();
            var sectionHeaderSize = externalSize == 0 ? 0x10u : 0x14u;
            if (sectionHeaderSize + languageSize + bankSize + soundSize + externalSize != headerSize)
                throw new InvalidDataException($"{path} has inconsistent AKPK section sizes.");
            stream.Position += languageSize;
            foreach (var entry in ReadTable(reader, path, bankSize, "bnk")) yield return entry;
            foreach (var entry in ReadTable(reader, path, soundSize, "wem")) yield return entry;
            stream.Position += externalSize;
        }

        private static IEnumerable<PckEntry> ReadTable(BinaryReader reader, string path, uint sectionSize, string kind)
        {
            if (sectionSize == 0) yield break;
            var start = reader.BaseStream.Position;
            var count = reader.ReadUInt32();
            if (count == 0)
            {
                reader.BaseStream.Position = start + sectionSize;
                yield break;
            }
            var entrySize = (sectionSize - 4) / count;
            if (4 + entrySize * count != sectionSize || entrySize is not (20 or 24))
                throw new InvalidDataException($"{path} has an invalid {kind} table.");
            for (var i = 0; i < count; i++)
            {
                var id = reader.ReadUInt32();
                var blockSize = reader.ReadUInt32();
                var size = entrySize == 24 ? reader.ReadUInt64() : reader.ReadUInt32();
                var startBlock = reader.ReadUInt32();
                reader.ReadUInt32();
                yield return new(path, kind, id, blockSize, size, blockSize == 0 ? startBlock : (ulong)startBlock * blockSize);
            }
        }

        private static (List<BankObject> Objects, List<MediaEntry> Media, long DataOffset) ReadBank(PckEntry entry, HashSet<uint> candidates)
        {
            using var stream = File.OpenRead(entry.Package);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            stream.Position = (long)entry.Offset;
            var end = checked((long)(entry.Offset + entry.Size));
            var objects = new List<BankObject>();
            var media = new List<MediaEntry>();
            long dataOffset = -1;
            while (stream.Position + 8 <= end)
            {
                var id = Encoding.ASCII.GetString(reader.ReadBytes(4));
                var size = reader.ReadUInt32();
                var chunkEnd = checked(stream.Position + size);
                if (chunkEnd > end) throw new InvalidDataException($"Bank {entry.Id} has an invalid {id} chunk.");
                if (id == "DIDX")
                {
                    if (size % 12 != 0) throw new InvalidDataException($"Bank {entry.Id} has an invalid DIDX chunk.");
                    while (stream.Position < chunkEnd)
                        media.Add(new(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32()));
                }
                else if (id == "DATA")
                    dataOffset = stream.Position;
                else if (id == "HIRC")
                {
                    var count = reader.ReadUInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var type = reader.ReadByte();
                        var length = reader.ReadUInt32();
                        if (length < 4 || stream.Position + length > chunkEnd) throw new InvalidDataException($"Bank {entry.Id} has an invalid HIRC object.");
                        var objectId = reader.ReadUInt32();
                        var payload = reader.ReadBytes((int)length - 4);
                        if (candidates.Contains(objectId) || type is 2 or 3 or 4 or 5 or 6 or 7 or 10 or 11 or 12 or 13)
                            objects.Add(new(type, objectId, payload));
                    }
                }
                stream.Position = chunkEnd;
            }
            return (objects, media, dataOffset);
        }

        private static IEnumerable<uint> ReadBankEventIds(PckEntry entry)
        {
            using var stream = File.OpenRead(entry.Package);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            stream.Position = (long)entry.Offset;
            var end = checked((long)(entry.Offset + entry.Size));
            while (stream.Position + 8 <= end)
            {
                var id = Encoding.ASCII.GetString(reader.ReadBytes(4));
                var size = reader.ReadUInt32();
                var chunkEnd = checked(stream.Position + size);
                if (chunkEnd > end) throw new InvalidDataException($"Bank {entry.Id} has an invalid {id} chunk.");
                if (id == "HIRC")
                {
                    var count = reader.ReadUInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var type = reader.ReadByte();
                        var length = reader.ReadUInt32();
                        if (length < 4 || stream.Position + length > chunkEnd) throw new InvalidDataException($"Bank {entry.Id} has an invalid HIRC object.");
                        var objectId = reader.ReadUInt32();
                        if (type == 4) yield return objectId;
                        stream.Position += length - 4;
                    }
                }
                stream.Position = chunkEnd;
            }
        }

        private static void AppendEventGraph(List<string> manifest, uint eventId, List<BankObject> objects)
        {
            var index = objects.GroupBy(obj => obj.Id).ToDictionary(group => group.Key, group => group.First());
            if (!index.TryGetValue(eventId, out var evt) || evt.Payload.Length < 4) return;
            var actionCount = BinaryPrimitives.ReadUInt32LittleEndian(evt.Payload);
            manifest.Add($"event={eventId} actions={actionCount}");
            for (var i = 0; i < actionCount && 4 + i * 4 + 4 <= evt.Payload.Length; i++)
            {
                var actionId = BinaryPrimitives.ReadUInt32LittleEndian(evt.Payload.AsSpan(4 + i * 4, 4));
                if (index.TryGetValue(actionId, out var action) && action.Type == 3 && action.Payload.Length >= 7)
                {
                    var actionType = BinaryPrimitives.ReadUInt16LittleEndian(action.Payload);
                    var targetId = BinaryPrimitives.ReadUInt32LittleEndian(action.Payload.AsSpan(2, 4));
                    manifest.Add($" action={actionId} type=0x{actionType:X4} target={targetId}");
                }
                else
                    manifest.Add($" action={actionId}");
            }
        }

        private static IReadOnlyList<string> WriteTxtps(string folder, ModuleItem tag, uint eventId, List<BankObject> objects, List<string> manifest)
        {
            var paths = new List<string>();
            var index = objects.GroupBy(obj => obj.Id).ToDictionary(group => group.Key, group => group.First());
            if (!index.TryGetValue(eventId, out var evt) || evt.Type != 4 || evt.Payload.Length < 8)
                return paths;
            var actionId = BinaryPrimitives.ReadUInt32LittleEndian(evt.Payload.AsSpan(4, 4));
            if (!index.TryGetValue(actionId, out var action) || action.Type != 3 || action.Payload.Length < 6)
                return paths;
            var actionType = BinaryPrimitives.ReadUInt16LittleEndian(action.Payload);
            if (actionType != 0x0403)
                return paths;
            var targetId = BinaryPrimitives.ReadUInt32LittleEndian(action.Payload.AsSpan(2, 4));
            if (!index.TryGetValue(targetId, out var musicTarget) || musicTarget.Type is not (10 or 12 or 13))
                return paths;
            var segments = FindReachableSegments(targetId, index);
            manifest.Add($" music_target={targetId} hirc_type={(index.TryGetValue(targetId, out var target) ? target.Type : -1)} reachable_segments={segments.Count}");
            if (target != null)
            {
                var references = FindObjectReferences(target.Payload, index);
                manifest.Add($"  target_references={string.Join(",", references.Select(id => $"{id}:{index[id].Type}"))}");
            }
            foreach (var segmentId in segments)
            {
                var trackCandidates = objects.Count(obj => obj.Type == 11 && ContainsId(obj.Payload, segmentId));
                var path = WriteSegmentTxtp(folder, tag, eventId, actionId, segmentId, objects);
                manifest.Add($"  segment={segmentId} track_candidates={trackCandidates} txtp={(path == null ? "no" : "yes")}");
                if (path != null)
                    paths.Add(path);
            }
            return paths;
        }

        private static string WriteSegmentTxtp(string folder, ModuleItem tag, uint eventId, uint actionId, uint segmentId, List<BankObject> objects)
        {
            var tracks = objects.Where(obj => obj.Type == 11 && ContainsId(obj.Payload, segmentId))
                .Select(obj => (Object: obj, Clips: ReadTrackClips(obj.Payload)))
                .Where(value => value.Clips.Count > 0)
                .ToList();
            if (tracks.Count == 0)
                return null;

            var allClips = tracks.SelectMany(track => track.Clips)
                .Select(clip => (Clip: clip, DurationMs: clip.SourceDurationMs + clip.EndTrimMs - clip.BeginTrimMs))
                .Where(value => value.DurationMs > 0)
                .ToList();
            var compositionEndMs = allClips.Count == 0 ? 0 : allClips.Max(value => value.Clip.PlayAtMs + value.DurationMs);
            var lines = new List<string>();
            foreach (var value in allClips)
            {
                var clip = value.Clip;
                var line = $"{clip.MediaId}.wem #i";
                if (clip.PlayAtMs > 0)
                    line += $" #p {FormatSeconds(clip.PlayAtMs)}";
                line += $" #b {FormatSeconds(value.DurationMs)}";
                var endPaddingMs = compositionEndMs - clip.PlayAtMs - value.DurationMs;
                if (endPaddingMs > 0.0001)
                    line += $" #P {FormatSeconds(endPaddingMs)}";
                if (clip.FadeStartMs.HasValue && clip.FadeDurationMs > 0)
                    line += $" #m0^1.0~0.0=H@-1~{FormatSeconds(clip.PlayAtMs + clip.FadeStartMs.Value)}+{FormatSeconds(clip.FadeDurationMs.Value)}~-1";
                lines.Add(line);
            }
            if (lines.Count == 0)
                return null;
            if (lines.Count > 1)
                lines.Add($"group = -L{lines.Count} #@layer-v");
            lines.Add(string.Empty);
            lines.Add($"# Halo 5 tag: {tag.TagName}.{tag.ClassName}");
            lines.Add($"# Wwise event: {eventId}");
            lines.Add($"# Play action: {actionId}");
            lines.Add($"# Music segment: {segmentId}");
            var path = Path.Combine(folder, $"{Utils.GetSafeFileName(tag.FileName)}_{eventId}_{segmentId}.txtp");
            File.WriteAllLines(path, lines);
            return path;
        }

        private static IReadOnlyList<uint> FindReachableSegments(uint targetId, IReadOnlyDictionary<uint, BankObject> objects)
        {
            var segments = new List<uint>();
            var pending = new Queue<uint>();
            var visited = new HashSet<uint>();
            pending.Enqueue(targetId);
            while (pending.TryDequeue(out var id))
            {
                if (!visited.Add(id) || !objects.TryGetValue(id, out var obj))
                    continue;
                if (obj.Type == 10)
                {
                    segments.Add(id);
                    continue;
                }
                for (var offset = 0; offset <= obj.Payload.Length - 4; offset++)
                {
                    var childId = BinaryPrimitives.ReadUInt32LittleEndian(obj.Payload.AsSpan(offset, 4));
                    if (objects.ContainsKey(childId) && !visited.Contains(childId))
                        pending.Enqueue(childId);
                }
            }
            return segments;
        }

        private static HashSet<uint> FindReachableMedia(IEnumerable<uint> eventIds, List<BankObject> objects, HashSet<uint> availableMediaIds)
        {
            var index = objects.GroupBy(obj => obj.Id).ToDictionary(group => group.Key, group => group.First());
            var pending = new Queue<(uint Id, int Depth)>();
            var visited = new HashSet<uint>();
            var mediaIds = new HashSet<uint>();
            foreach (var eventId in eventIds)
            {
                if (!index.TryGetValue(eventId, out var evt) || evt.Type != 4 || evt.Payload.Length < 4)
                    continue;
                var actionCount = BinaryPrimitives.ReadUInt32LittleEndian(evt.Payload);
                for (var i = 0; i < actionCount && 8 + i * 4 <= evt.Payload.Length; i++)
                {
                    var actionId = BinaryPrimitives.ReadUInt32LittleEndian(evt.Payload.AsSpan(4 + i * 4, 4));
                    if (index.TryGetValue(actionId, out var action) && action.Type == 3 && action.Payload.Length >= 6
                        && BinaryPrimitives.ReadUInt16LittleEndian(action.Payload) == 0x0403)
                        pending.Enqueue((BinaryPrimitives.ReadUInt32LittleEndian(action.Payload.AsSpan(2, 4)), 0));
                }
            }

            while (pending.TryDequeue(out var pendingObject))
            {
                var (objectId, depth) = pendingObject;
                if (!visited.Add(objectId) || !index.TryGetValue(objectId, out var obj))
                    continue;
                if (obj.Type == 2)
                {
                    for (var offset = 0; offset <= obj.Payload.Length - 4; offset++)
                    {
                        var mediaId = BinaryPrimitives.ReadUInt32LittleEndian(obj.Payload.AsSpan(offset, 4));
                        if (availableMediaIds.Contains(mediaId))
                            mediaIds.Add(mediaId);
                    }
                    continue;
                }
                for (var offset = 0; offset <= obj.Payload.Length - 4; offset++)
                {
                    var id = BinaryPrimitives.ReadUInt32LittleEndian(obj.Payload.AsSpan(offset, 4));
                    if (depth < 2 && index.TryGetValue(id, out var child) && child.Type is 2 or 5 or 6 or 7 && !visited.Contains(id))
                        pending.Enqueue((id, depth + 1));
                }
            }
            return mediaIds;
        }

        private static IReadOnlyList<uint> FindObjectReferences(byte[] payload, IReadOnlyDictionary<uint, BankObject> objects)
        {
            var references = new HashSet<uint>();
            for (var offset = 0; offset <= payload.Length - 4; offset++)
            {
                var id = BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(offset, 4));
                if (objects.ContainsKey(id))
                    references.Add(id);
            }
            return references.ToList();
        }

        private static List<TrackClip> ReadTrackClips(byte[] payload)
        {
            var clips = new List<TrackClip>();
            var offset = 0;
            if (!TrySkip(payload, ref offset, 1) || !TryReadUInt32(payload, ref offset, out var sourceCount) || sourceCount > 1024)
                return clips;
            var sourceIds = new HashSet<uint>();
            for (var i = 0; i < sourceCount; i++)
            {
                if (!TrySkip(payload, ref offset, 5) || !TryReadUInt32(payload, ref offset, out var sourceId)
                    || !TrySkip(payload, ref offset, 13))
                    return new();
                sourceIds.Add(sourceId);
            }
            if (!TryReadUInt32(payload, ref offset, out var playlistCount) || playlistCount > 4096)
                return new();
            for (var i = 0; i < playlistCount; i++)
            {
                if (!TrySkip(payload, ref offset, 4) || !TryReadUInt32(payload, ref offset, out var sourceId)
                    || !TryReadDouble(payload, ref offset, out var playAt) || !TryReadDouble(payload, ref offset, out var beginTrim)
                    || !TryReadDouble(payload, ref offset, out var endTrim) || !TryReadDouble(payload, ref offset, out var duration))
                    return new();
                if (sourceIds.Contains(sourceId))
                    clips.Add(new(sourceId, playAt, beginTrim, endTrim, duration));
            }
            if (!TryReadUInt32(payload, ref offset, out _)
                || !TryReadUInt32(payload, ref offset, out var automationCount) || automationCount > 4096)
                return new();
            for (var i = 0; i < automationCount; i++)
            {
                if (!TryReadUInt32(payload, ref offset, out var clipIndex)
                    || !TryReadUInt32(payload, ref offset, out var automationType)
                    || !TryReadUInt32(payload, ref offset, out var pointCount) || pointCount > 4096)
                    return new();
                var points = new List<(float Time, float Value)>((int)pointCount);
                for (var point = 0; point < pointCount; point++)
                {
                    if (!TryReadSingle(payload, ref offset, out var time) || !TryReadSingle(payload, ref offset, out var value)
                        || !TrySkip(payload, ref offset, 4))
                        return new();
                    points.Add((time, value));
                }
                if (automationType == 4 && clipIndex < clips.Count && points.Count >= 2)
                {
                    var fadeStart = points[^2].Time * 1000d;
                    var fadeDuration = (points[^1].Time - points[^2].Time) * 1000d;
                    clips[(int)clipIndex] = clips[(int)clipIndex] with { FadeStartMs = fadeStart, FadeDurationMs = fadeDuration };
                }
            }
            return clips;
        }

        private static void ConvertToWav(string executable, string txtpPath, string wavPath)
        {
            var startInfo = new ProcessStartInfo(executable)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(txtpPath)
            };
            startInfo.ArgumentList.Add("-o");
            startInfo.ArgumentList.Add(wavPath);
            startInfo.ArgumentList.Add(txtpPath);
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Unable to start vgmstream.");
            process.WaitForExit();
            if (process.ExitCode != 0 || !File.Exists(wavPath))
                throw new InvalidOperationException($"vgmstream failed with exit code {process.ExitCode}.");
        }

        private static bool TryConvertToWav(string executable, string inputPath, string wavPath, List<string> manifest)
        {
            try
            {
                ConvertToWav(executable, inputPath, wavPath);
                return true;
            }
            catch (Exception ex)
            {
                manifest.Add($"conversion_failed={Path.GetFileName(inputPath)} error={ex.Message}");
                return false;
            }
        }

        private static void DeleteIntermediateMedia(IEnumerable<string> mediaPaths, List<string> manifest)
        {
            foreach (var mediaPath in mediaPaths.Where(File.Exists))
            {
                File.Delete(mediaPath);
                manifest.Add($"removed_intermediate={Path.GetFileName(mediaPath)}");
            }
        }

        private static void ArchiveUnconvertedMedia(string folder, IEnumerable<string> mediaPaths, List<string> manifest)
        {
            var paths = mediaPaths.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (paths.Count == 0)
                return;
            var archivePath = Path.Combine(folder, "unconverted_wem.zip");
            if (File.Exists(archivePath))
                File.Delete(archivePath);
            using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                foreach (var path in paths)
                    archive.CreateEntryFromFile(path, Path.GetFileName(path), CompressionLevel.SmallestSize);
            foreach (var path in paths)
                File.Delete(path);
            manifest.Add($"unconverted_archive={Path.GetFileName(archivePath)} files={paths.Count}");
        }

        private static void DeleteSourceBanks(IEnumerable<string> bankPaths, List<string> manifest)
        {
            foreach (var bankPath in bankPaths.Where(File.Exists))
            {
                File.Delete(bankPath);
                manifest.Add($"removed_source_bank={Path.GetFileName(bankPath)}");
            }
        }

        private static bool ContainsId(byte[] payload, uint id)
        {
            for (var offset = 0; offset <= payload.Length - 4; offset++)
                if (BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(offset, 4)) == id)
                    return true;
            return false;
        }

        private static bool TrySkip(byte[] data, ref int offset, int count)
        {
            if (offset < 0 || count < 0 || offset + count > data.Length) return false;
            offset += count;
            return true;
        }

        private static bool TryReadUInt32(byte[] data, ref int offset, out uint value)
        {
            value = 0;
            if (offset < 0 || offset + 4 > data.Length) return false;
            value = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4));
            offset += 4;
            return true;
        }

        private static bool TryReadDouble(byte[] data, ref int offset, out double value)
        {
            value = 0;
            if (offset < 0 || offset + 8 > data.Length) return false;
            value = BitConverter.ToDouble(data, offset);
            offset += 8;
            return double.IsFinite(value);
        }

        private static bool TryReadSingle(byte[] data, ref int offset, out float value)
        {
            value = 0;
            if (offset < 0 || offset + 4 > data.Length) return false;
            value = BitConverter.ToSingle(data, offset);
            offset += 4;
            return float.IsFinite(value);
        }

        private static string FormatSeconds(double milliseconds) => (milliseconds / 1000d).ToString("0.###############", CultureInfo.InvariantCulture);

        private static void CopyEntry(PckEntry entry, string outputPath)
        {
            using var source = File.OpenRead(entry.Package);
            source.Position = (long)entry.Offset;
            CopyRange(source, outputPath, checked((long)entry.Size));
        }

        private static void CopyEmbeddedMedia(PckEntry bank, long absoluteOffset, uint size, string outputPath)
        {
            var bankEnd = checked((long)(bank.Offset + bank.Size));
            if (absoluteOffset < (long)bank.Offset || absoluteOffset + size > bankEnd)
                throw new InvalidDataException($"Embedded media range is outside bank {bank.Id}.");
            using var source = File.OpenRead(bank.Package);
            source.Position = absoluteOffset;
            CopyRange(source, outputPath, size);
        }

        private static void CopyRange(Stream source, string outputPath, long size)
        {
            using var output = File.Create(outputPath);
            var buffer = new byte[1024 * 1024];
            while (size > 0)
            {
                var read = source.Read(buffer, 0, (int)Math.Min(buffer.Length, size));
                if (read == 0) throw new EndOfStreamException();
                output.Write(buffer, 0, read);
                size -= read;
            }
        }

        private static uint WwiseHash(string value)
        {
            uint hash = 2166136261;
            foreach (var c in value.ToLowerInvariant()) hash = unchecked(hash * 16777619) ^ (byte)c;
            return hash;
        }

        private static string SafeRelativePath(string value) => Path.Combine(value.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries).Select(Utils.GetSafeFileName).ToArray());
    }
}
