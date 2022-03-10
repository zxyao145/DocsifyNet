using Microsoft.Extensions.Options;
using System.Text;

namespace DocsifyNet.Module
{
    internal class DirNode
    {
        /// <summary>
        /// 父目录
        /// </summary>
        public DirNode? Parent { get; set; } = null;

        /// <summary>
        /// 相对home目录层级
        /// </summary>
        public int RelativeLevel { get; set; } = 0;

        public string CurDirName { get; set; } = "";

        public List<string> Files { get; set; } = new List<string>();

        public List<DirNode> Dirs { get; set; } = new List<DirNode>();

        public override string ToString()
        {
            return $"{RelativeLevel}:{CurDirName}";
        }
    }

    public class SidebarCreatorOption
    {
        public bool GenerateSideBarInSubDir { get; set; } = false;
        public string IndexFileName { get; set; } = "README.md";

        public void Verify()
        {
            if (string.IsNullOrWhiteSpace(IndexFileName))
            {
                IndexFileName = "README.md";
            }
        }
    }

    public class SidebarCreator
    {
        const string SideBarFileName = "_sidebar.md";

        private readonly IWebHostEnvironment _env;

        private SidebarCreatorOption _options;

        public SidebarCreator(IWebHostEnvironment env, IOptionsMonitor<SidebarCreatorOption> optionsMonitor)
        {
            _env = env;
            _options = optionsMonitor.CurrentValue;
            _options.Verify();
            optionsMonitor.OnChange((options) =>
            {
                _options = options;
                _options.Verify();
            });
        }

        private static List<string> _ignoreDirList = new List<string>()
        {
            "assets",
        };

        private const int level = 3;
        private string _homeDir = "";

        public async Task<bool> RunAsync()
        {
            _homeDir = Path.Combine(_env.WebRootPath, "docs");
            DirNode root = WalkDir(_homeDir);
            await WriteToSidebarFileAsync(root);
            return true;
        }

        /// <summary>
        /// 生成md列表缩进的空格
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private string GenLeveSpace(int level)
        {
            StringBuilder sb = new StringBuilder();
            while (level-- > 0)
            {
                sb.Append("  ");
            }
            return sb.ToString();
        }

        private string GenMdLink(string relativeFileName, string? dirName = null)
        {
            string linkAddress = "";
            string linkShowName = Path.GetFileNameWithoutExtension(relativeFileName);

            if (relativeFileName.EndsWith(".md"))
            {
                linkAddress = relativeFileName;
            }
            else
            {
                linkAddress = $"{relativeFileName}/{_options.IndexFileName}";
            }
            linkAddress = linkAddress.Replace(" ", "%20");
            return $"- [{dirName ?? linkShowName}]({linkAddress}){Environment.NewLine}";
        }

        private async Task<List<string>> WriteToSidebarFileAsync(DirNode node, string relativeDirName = "")
        {
            var curPathFiles = new List<string>(node.Files.Select(e => GenMdLink($"{relativeDirName}{e}")));
            if (node.Dirs.Any())
            {

                foreach (var subDir in node.Dirs)
                {
                    var relativeDirNameTemp = node.RelativeLevel == 0
                            ? $"{subDir.CurDirName}/"
                            : $"{relativeDirName}{subDir.CurDirName}/";

                    curPathFiles.Add(GenMdLink($"{relativeDirName}{subDir.CurDirName}", subDir.CurDirName));
                    var subPathFiles = await WriteToSidebarFileAsync(subDir, relativeDirNameTemp);
                    curPathFiles.AddRange(subPathFiles);
                }
            }

            var curDirFullPath = Path.Combine(_homeDir, relativeDirName);
            var sidebarFile = Path.Combine(curDirFullPath, SideBarFileName);

            if (_options.GenerateSideBarInSubDir)
            {
                await File.WriteAllLinesAsync(sidebarFile, curPathFiles);
            }
            else if (node.RelativeLevel == 0)
            {
                curPathFiles.Insert(0, $"- [首页]({_options.IndexFileName}){Environment.NewLine}");
                await File.WriteAllLinesAsync(sidebarFile, curPathFiles);
            }

            if (Directory.GetFiles(curDirFullPath, _options.IndexFileName).Length == 0)
            {
                var indexFile = Path.Combine(curDirFullPath, _options.IndexFileName);

                // md 以 \n 换行，无论是否是win环境
                await File.WriteAllLinesAsync(indexFile, curPathFiles);
            }

            string levelSpace = GenLeveSpace(node.RelativeLevel);
            return curPathFiles.Select(e => $"  {e}").ToList();
        }

        /// <summary>
        /// 遍历目录
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="parentNode"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private DirNode WalkDir(
            string dirName,
            DirNode? parentNode = null,
            int level = 0)
        {
            // 获取目录中所有的文件夹和文件
            var fileList = Directory.GetFiles(dirName, "*.md")
                .Select(e => Path.GetFileName(e))
                .Where(e => !(e.StartsWith("_") || e.Equals(_options.IndexFileName)))
                .ToList();

            DirNode curDirNode = new DirNode()
            {
                Parent = parentNode,
                RelativeLevel = level,
                CurDirName = level == 0 ? "" : new DirectoryInfo(dirName).Name,
                Files = fileList,
            };

            var dirList = Directory.GetDirectories(dirName);

            foreach (var dirItem in dirList)
            {
                var curDirName = Path.GetFileName(dirItem);
                if (curDirName.StartsWith(".")
                    || curDirName.StartsWith("_")
                    || _ignoreDirList.Contains(curDirName))
                {
                    continue;
                }

                var node = WalkDir(dirItem, curDirNode, level + 1);
                curDirNode.Dirs.Add(node);
            }

            return curDirNode;
        }
    }
}
