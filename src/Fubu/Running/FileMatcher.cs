using System.Collections.Generic;
using FubuCore.Util;
using System.Linq;

namespace Fubu.Running
{
    public class FileMatcher
    {
        private readonly Cache<FileChangeCategory, IList<IFileMatch>> _matchers = new Cache<FileChangeCategory, IList<IFileMatch>>(x => new List<IFileMatch>());
        private readonly Cache<string, FileChangeCategory> _results;

        public FileMatcher()
        {
            _results = new Cache<string, FileChangeCategory>(file => {
                if (matches(FileChangeCategory.AppDomain, file)) return FileChangeCategory.AppDomain;
                if (matches(FileChangeCategory.Application, file)) return FileChangeCategory.Application;
                if (matches(FileChangeCategory.Content, file)) return FileChangeCategory.Content;
                return FileChangeCategory.Nothing;
            });

            Add(new BinFileMatch());
            Add(new ExactFileMatch(FileChangeCategory.AppDomain, "web.config"));
            Add(new ExtensionMatch(FileChangeCategory.AppDomain, "*.exe"));
            Add(new ExtensionMatch(FileChangeCategory.AppDomain, "*.dll"));
        }

        public FileChangeCategory CategoryFor(string file)
        {
            return _results[file];
        }

        private bool matches(FileChangeCategory category, string file)
        {
            var matchers = _matchers[category];
            return matchers.Any(x => x.Matches(file));
        }

        public void Add(IFileMatch match)
        {
            _matchers[match.Category].Add(match);
        }
    }
}