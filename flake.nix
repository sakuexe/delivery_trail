{
  # using zsh, enter the shell using: nix develop --command zsh
  description = "Development flake for Unity and Neovim";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-24.05";
    utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, utils }:
    utils.lib.eachDefaultSystem(system:
      let
        pkgs = import nixpkgs { 
          inherit system; 
          config.allowUnfree = true; # enable the use of proprietary packages
        };
      in
      {
        devShell = with pkgs; mkShell {
          buildInputs = [
            (unityhub.override { extraLibs = { ... }: [ harfbuzz ]; })
            dotnetCorePackages.dotnet_8.sdk
            dotnetCorePackages.dotnet_8.runtime
            csharp-ls
            neovim
          ];
          shellHook = ''
            VIOLET='\033[0;35m'
            CLEAR='\033[0m'
            echo -e "💽$VIOLET Unity development shell $CLEAR"
            echo "starting UnityHub in the background..."
            unityhub > /dev/null 2>&1 &
          '';
        };
      });
}
