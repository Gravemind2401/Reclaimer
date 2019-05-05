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
        private const string query = @"
        select t.*, p.value
        from tag_item t
        join ( select t.cache_id,
                      t.tag_id,
                      row_number() over (partition by class_code, path_id order by c.priority desc) as rownum
               from tag_item t
               join cache_file c on t.cache_id = c.cache_id
               where c.cache_type = @cache_type
               and c.build_string = @build_string
               and ( @map_id = -1 or c.cache_id = @map_id ) ) x on t.cache_id = x.cache_id and t.tag_id = x.tag_id
        join tag_path p on t.path_id = p.path_id
        where rownum = 1";

        internal static readonly DatabaseContext Context = new DatabaseContext();

        public static IQueryable<CacheFile> CacheFiles => Context.CacheFiles;

        public static IQueryable<TagItem> IndexItems => Context.TagItems;

        public static IEnumerable<TagItem> IndexItemsFor(CacheType game, string build, int mapId)
        {
            var gameParam = new System.Data.SQLite.SQLiteParameter("cache_type", (int)game);
            var buildParam = new System.Data.SQLite.SQLiteParameter("build_string", build);
            var mapParam = new System.Data.SQLite.SQLiteParameter("map_id", mapId);

            var results = new List<TagItem>();
            Context.Database.Connection.Open();
            using (var cmd = Context.Database.Connection.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.Parameters.Add(gameParam);
                cmd.Parameters.Add(buildParam);
                cmd.Parameters.Add(mapParam);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new TagItem
                        {
                            CacheId = reader.GetInt32(0),
                            TagId = reader.GetInt32(1),
                            MetaPointer = reader.GetInt32(2),
                            PathId = reader.GetInt32(3),
                            ClassCode = reader.GetString(4),
                            TagPath = new TagPath
                            {
                                PathId = reader.GetInt32(3),
                                Value = reader.GetString(5)
                            }
                        };
                        results.Add(item);
                    }
                }
            }
            Context.Database.Connection.Close();

            return results;
        }

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
