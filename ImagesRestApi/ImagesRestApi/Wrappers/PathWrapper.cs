#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImagesRestApi.Wrappers
{
    public interface IPathWrapper
    {
        string? GetDirectoryName(string? path);
        string? GetExtension(string? path);
        string Combine(params string[] paths);
    }

    public class PathWrapper : IPathWrapper
    {
        public string? GetDirectoryName(string? path) => Path.GetDirectoryName(path);
        public string? GetExtension(string? path) => Path.GetExtension(path);
        public string Combine(params string[] paths) => Path.Combine(paths);
    }
}
