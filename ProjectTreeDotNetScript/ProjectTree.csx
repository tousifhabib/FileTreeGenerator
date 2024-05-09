using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class ProjectTree
{
    private readonly string projectType;
    private readonly List<string> ignorePatterns;
    private readonly string indentStyle;

    public ProjectTree(string projectType, string indentStyle = "tree")
    {
        this.projectType = projectType;
        this.ignorePatterns = GetIgnorePatterns();
        this.indentStyle = indentStyle;
    }

    private List<string> GetIgnorePatterns()
    {
        var commonPatterns = new List<string> { ".git", "coverage", ".DS_Store", "Thumbs.db", ".idea", ".vscode" };
        var projectSpecificPatterns = new Dictionary<string, List<string>>
        {
            ["python"] = new List<string> { ".venv", "__pycache__" },
            ["nodejs"] = new List<string> { "node_modules" },
            ["vue"] = new List<string> { "node_modules", "dist" },
            ["react"] = new List<string> { "node_modules", "build" },
            ["ruby"] = new List<string> { ".bundle", "vendor" },
            ["rails"] = new List<string> { "vendor", "log", "tmp" }
        };

        if (projectSpecificPatterns.TryGetValue(projectType, out var patterns))
        {
            commonPatterns.AddRange(patterns);
        }

        return commonPatterns;
    }

    private bool ShouldIgnore(string path)
    {
        return ignorePatterns.Any(pattern => path.Contains(pattern));
    }

    private string GetIndentStyle(int level, bool isLast)
    {
        if (indentStyle == "tree")
        {
            if (level == 0) return "";

            var indent = new StringBuilder();
            for (int i = 0; i < level - 1; i++)
            {
                indent.Append("│   ");
            }
            indent.Append(isLast ? "└── " : "├── ");
            return indent.ToString();
        }
        else
        {
            return indentStyle switch
            {
                "dash" => new string('-', 4 * level),
                "dot" => new string('.', 4 * level),
                _ => new string(' ', 4 * level),
            };
        }
    }

    public void DisplayTree(string rootDir)
    {
        if (!Directory.Exists(rootDir))
        {
            Console.WriteLine("Error: Project path does not exist");
            return;
        }

        rootDir = rootDir.TrimEnd(Path.DirectorySeparatorChar);
        Console.WriteLine(rootDir + Path.DirectorySeparatorChar);

        DisplayDirectoryContents(rootDir, 0);
    }

    private void DisplayDirectoryContents(string dir, int level)
    {
        var subDirs = Directory.GetDirectories(dir)
            .Where(d => !ShouldIgnore(d))
            .OrderBy(d => d)
            .ToList();
        var files = Directory.GetFiles(dir)
            .Where(f => !ShouldIgnore(f))
            .OrderBy(f => f)
            .ToList();

        int totalItems = subDirs.Count + files.Count;
        for (int i = 0; i < totalItems; i++)
        {
            bool isLast = (i == totalItems - 1);
            if (i < subDirs.Count)
            {
                // Handle subdirectory
                string subDir = subDirs[i];
                string indent = GetIndentStyle(level + 1, isLast);
                Console.WriteLine($"{indent}{new DirectoryInfo(subDir).Name}{Path.DirectorySeparatorChar}");
                DisplayDirectoryContents(subDir, level + 1);
            }
            else
            {
                // Handle file
                string file = files[i - subDirs.Count];
                string indent = GetIndentStyle(level + 1, isLast);
                Console.WriteLine($"{indent}{Path.GetFileName(file)}");
            }
        }
    }
}

string[] args = Environment.GetCommandLineArgs();
string projectPath, projectType, indentStyle;

if (args.Length >= 4)
{
    projectPath = args[1];
    projectType = args[2];
    indentStyle = args[3];
}
else
{
    Console.WriteLine("Interactive mode: Please enter the details.");
    Console.Write("Enter project path: ");
    projectPath = Console.ReadLine();
    Console.Write("Enter project type (python, nodejs, vue, react, ruby, rails): ");
    projectType = Console.ReadLine();
    Console.Write("Enter indent style (space, dash, dot, tree): ");
    indentStyle = Console.ReadLine();
}

var projectTree = new ProjectTree(projectType, indentStyle);
projectTree.DisplayTree(projectPath);
