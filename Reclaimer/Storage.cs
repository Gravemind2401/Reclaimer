using Adjutant.Blam.Definitions;
using Adjutant.Utilities;
using Reclaimer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer
{
    public static class Storage
    {
        internal static readonly DatabaseContext Context = new DatabaseContext();

        public static IQueryable<CacheFile> CacheFiles => Context.CacheFiles;

        public static IQueryable<TagItem> IndexItems => Context.TagItems;

        private static long PathHash(string path)
        {
            return ((long)MurMur3.Hash32(path) << 32) | MurMur3.Hash32(new string(path.Reverse().ToArray()));
        }

        public static async Task ImportCacheFile(string fileName)
        {
            ICacheFile cache;
            try
            {
                cache = Adjutant.Blam.CacheFactory.ReadCacheFile(fileName);
            }
            catch
            {
                System.Windows.MessageBox.Show("bad map: " + fileName);
                return;
            }

            var entity = Context.CacheFiles.Create();

            entity.FileName = cache.FileName;
            entity.BuildString = cache.BuildString;
            entity.CacheType = cache.Type;

            var tagIndex = Context.TagIndexes.Create();
            tagIndex.Magic = cache.TagIndex.Magic;
            tagIndex.TagCount = cache.TagIndex.TagCount;
            tagIndex.CacheFile = entity;

            var allIds = cache.TagIndex.Select(t => PathHash(t.FileName)).ToArray();

            var existingPaths = Context.TagPaths
                .Where(p => allIds.Contains(p.PathId))
                .ToDictionary(p => p.PathId);

            var newPaths = new Dictionary<long, TagPath>();
            foreach (var tag in cache.TagIndex)
            {
                using (new DiagnosticTimer($"tag {tag.Id}"))
                {
                    var item = Context.TagItems.Create();
                    item.TagId = tag.Id;
                    item.MetaPointer = tag.MetaPointer.Value;
                    item.ClassCode = tag.ClassCode;

                    var hash = PathHash(tag.FileName);
                    TagPath path;

                    if (newPaths.ContainsKey(hash))
                        path = newPaths[hash];
                    else if (existingPaths.ContainsKey(hash))
                        path = existingPaths[hash];
                    else
                    {
                        path = Context.TagPaths.Create();
                        path.PathId = hash;
                        path.Value = tag.FileName;
                        newPaths.Add(hash, path);
                    }

                    if (path.Value != tag.FileName)
                    {
                        System.Diagnostics.Debugger.Break();
                        throw new ArgumentException("filename hash collision!");
                    }

                    item.TagPath = path;
                    tagIndex.TagItems.Add(item);
                }
            }

            if (cache.StringIndex != null)
            {
                var stringIndex = Context.StringIndexes.Create();
                stringIndex.StringCount = cache.StringIndex.StringCount;
                stringIndex.CacheFile = entity;

                allIds = cache.StringIndex
                    .Where(s => s.Value != null)
                    .Select(s => PathHash(s.Value))
                    .Distinct()
                    .ToArray();

                var existingStrings = Context.StringValues
                    .Where(s => allIds.Contains(s.ValueId))
                    .ToDictionary(s => s.ValueId);

                var newStrings = new Dictionary<long, StringValue>();
                foreach (var str in cache.StringIndex)
                {
                    var item = Context.StringItems.Create();
                    item.StringId = str.Id;

                    if (str.Value != null)
                    {
                        var hash = PathHash(str.Value);
                        StringValue val;

                        if (newStrings.ContainsKey(hash))
                            val = newStrings[hash];
                        else if (existingStrings.ContainsKey(hash))
                            val = existingStrings[hash];
                        else
                        {
                            val = Context.StringValues.Create();
                            val.ValueId = hash;
                            val.Value = str.Value;
                            newStrings.Add(hash, val);
                        }

                        if (val.Value != str.Value)
                        {
                            System.Diagnostics.Debugger.Break();
                            throw new ArgumentException("string hash collision!");
                        }

                        item.StringValue = val;
                    }

                    stringIndex.StringItems.Add(item);
                }

                Context.StringIndexes.Add(stringIndex);
                Context.StringItems.AddRange(stringIndex.StringItems);
                Context.StringValues.AddRange(newStrings.Values);
            }

            Context.CacheFiles.Add(entity);
            Context.TagIndexes.Add(tagIndex);

            Context.TagItems.AddRange(tagIndex.TagItems);
            Context.TagPaths.AddRange(newPaths.Values);

            await Context.SaveChangesAsync();
        }
    }

    public class DiagnosticTimer : IDisposable
    {
        private readonly string caller;
        private readonly DateTime start;

        public DiagnosticTimer([CallerMemberName] string caller = null)
        {
            this.caller = caller;
            start = DateTime.Now;
        }

        public void Dispose()
        {
            var end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine($"{caller} took {(end - start).TotalMilliseconds}ms");
        }
    }
}
