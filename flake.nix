{
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
          ];
          shellHook = ''
            zsh
          '';
        };
      });
}
