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

        public static IQueryable<IndexItem> IndexItems => Context.IndexItems;

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
            tagIndex.Magic = ((Adjutant.Blam.Halo1.CacheFile)cache).TagIndex.Magic;
            tagIndex.TagCount = cache.TagIndex.TagCount;
            tagIndex.CacheFile = entity;

            var allIds = cache.TagIndex.Select(t => PathHash(t.FileName)).ToArray();

            var existingPaths = Context.Paths
                .Where(p => allIds.Contains(p.PathId))
                .ToDictionary(p => p.PathId);

            var newPaths = new Dictionary<long, Path>();
            foreach (var tag in cache.TagIndex)
            {
                using (new DiagnosticTimer($"tag {tag.Id}"))
                {
                    var item = Context.IndexItems.Create();
                    item.TagId = tag.Id;
                    item.MetaPointer = tag.MetaPointer.Value;
                    item.ClassCode = tag.ClassCode;

                    var hash = PathHash(tag.FileName);
                    Path path;

                    if (newPaths.ContainsKey(hash))
                        path = newPaths[hash];
                    else
                    {
                        if (existingPaths.ContainsKey(hash))
                            path = existingPaths[hash];
                        else
                        {
                            path = Context.Paths.Create();
                            path.PathId = hash;
                            path.Value = tag.FileName;
                            newPaths.Add(hash, path);
                        }
                    }

                    if (path.Value != tag.FileName)
                    {
                        System.Diagnostics.Debugger.Break();
                        throw new ArgumentException("filename hash collision!");
                    }

                    item.Path = path;

                    tagIndex.IndexItems.Add(item);
                }
            }

            Context.IndexItems.AddRange(tagIndex.IndexItems);
            Context.Paths.AddRange(newPaths.Values);

            Context.CacheFiles.Add(entity);
            Context.TagIndexes.Add(tagIndex);

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
