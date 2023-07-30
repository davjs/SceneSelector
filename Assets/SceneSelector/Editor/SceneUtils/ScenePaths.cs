using System;
using System.IO;

namespace SceneSelector.Editor.SceneUtils {
    static class ScenePaths {
        public static string GetSceneDisplayPath(string path) {
            var normalizedPath = path.Replace('\\', '/') ?? "";
            var prefixRemoved = RemovePrefixFromPath(normalizedPath);
            var postFixRemoved = RemovePostfixFromPath(prefixRemoved,  Path.GetExtension(path) ?? ".unity");
            return postFixRemoved;
        }

        private static string RemovePostfixFromPath(string path, string extension) {
            return path switch {
                {} when path.EndsWith(extension) => path[..^".unity".Length],
                _ => path
            } ?? throw new InvalidOperationException();
        }

        private static string RemovePrefixFromPath(string path) {
            var pathLowercase = path.ToLower().Replace("\\", "/");
            return pathLowercase switch {
                { } when pathLowercase.StartsWith("assets/scenes/") => path["assets/scenes/".Length..],
                { } when pathLowercase.StartsWith("assets/") => path["assets/".Length..],
                _ => path
            };
        }
    }
}