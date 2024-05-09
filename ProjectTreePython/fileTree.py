import os
import sys


class ProjectTree:
    def __init__(self, project_type, indent_style='tree'):
        self.project_type = project_type
        self.ignore_patterns = self.get_ignore_patterns()
        self.indent_style = indent_style

    def get_ignore_patterns(self):
        # Define ignore patterns for different project types
        common_patterns = [
            '.git',         # Git VCS
            'coverage',     # Test coverate reports
            '.DS_Store',    # macOS specific
            'Thumbs.db',    # Windows specific
            '.idea',        # JetBrains IDEs
            '.vscode'       # Visual Studio Code
        ]
        project_specific_patterns = {
            'python': ['.venv', '__pycache__'],
            'nodejs': ['node_modules'],
            'vue': ['node_modules', 'dist'],
            'react': ['node_modules', 'build'],
            'ruby': ['.bundle'],
            'rails': ['vendor', 'log', 'tmp']
        }
        return common_patterns + project_specific_patterns.get(self.project_type, [])

    def should_ignore(self, path):
        for pattern in self.ignore_patterns:
            if pattern in path:
                return True
        return False

    def get_indent_style(self, level, is_last):
        if self.indent_style == 'dash':
            return '-' * 4 * level
        elif self.indent_style == 'dot':
            return '.' * 4 * level
        elif self.indent_style == 'tree':
            if level == 0:
                return ''
            else:
                return ('│   ' * (level - 1)) + ('└── ' if is_last else '├── ')
        else:
            return '    ' * level

    def display_tree(self, root_dir):
        if not os.path.exists(root_dir):
            print("Error: Project path does not exist")
            return

        root_dir = root_dir.rstrip(os.path.sep)
        start = root_dir.rfind(os.path.sep) + 1
        print(root_dir + '/')
        for root, dirs, files in os.walk(root_dir):
            dirs[:] = [d for d in dirs if not self.should_ignore(os.path.join(root, d))]
            files[:] = [f for f in files if not self.should_ignore(os.path.join(root, f))]

            level = root[start:].count(os.sep)
            indent = self.get_indent_style(level, False)

            if root != root_dir:
                print(f"{indent}{os.path.basename(root)}/")

            sub_indent = self.get_indent_style(level + 1, False)
            for name in sorted(dirs + files):
                if os.path.isdir(os.path.join(root, name)):
                    continue
                else:
                    print(f"{sub_indent}{name}")

def main():
    if len(sys.argv) >= 3:
        project_path = sys.argv[1]
        project_type = sys.argv[2]
        indent_style = sys.argv[3] if len(sys.argv) > 3 else 'space'
    else:
        print("Interactive mode: Please enter the details.")
        project_path = input("Enter project path: ")
        project_type = input("Enter project type (python, nodejs, vue, react, ruby, rails): ")
        indent_style = input("Enter indent style (space, dash, dot, tree): ")

    project_tree = ProjectTree(project_type, indent_style)
    project_tree.display_tree(project_path)


if __name__ == "__main__":
    main()
