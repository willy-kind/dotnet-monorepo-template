{
  description = "Dev environment for a .NET mono repo";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs { inherit system; };
        dotnet = pkgs.dotnet-sdk_10;
      in {
        devShells.default = pkgs.mkShell {
          buildInputs = with pkgs; [
            bash-completion
            git
            dotnet
            # add project specific tools here
          ];

          shellHook = ''
            source ${pkgs.bash-completion}/etc/profile.d/bash_completion.sh

            # dotnet
            export DOTNET_ROOT="${dotnet}/share/dotnet"
            export DOTNET_CLI_TELEMETRY_OPTOUT=1
            export DOTNET_NOLOGO=1
            export PATH="$HOME/.dotnet/tools:$PATH"
            _dotnet_bash_complete() {
              local IFS=$'\n'
              COMPREPLY=($(dotnet complete --position "''${COMP_POINT}" "''${COMP_LINE}" 2>/dev/null))
            }
            complete -f -F _dotnet_bash_complete dotnet

            # use the repo's git hooks (conventional commits)
            git config core.hooksPath .githooks

            export DEVENV="MonoDotnet"
            echo "Development environment is ready 󱓟 "
          '';
        };
      });
}
