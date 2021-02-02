{ pkgs ? import <nixpkgs> {} }:

let
  env = pkgs.buildFHSUserEnv {
    name = "document-generation";
    targetPkgs = pkgs: with pkgs; [
      dotnet-sdk_3
    ];
    extraOutputsToInstall = [ "dev" ];
    profile = ''
      export DOTNET_ROOT=${pkgs.dotnet-sdk_3}
    '';
    runScript = pkgs.writeScript "env-shell" ''
      #!${pkgs.stdenv.shell}
      exec ${userShell}
    '';
  };

  userShell = builtins.getEnv "SHELL";

in pkgs.stdenv.mkDerivation {
  name = "document-generation-fhs-dev";

  shellHook = ''
    exec ${env}/bin/document-generation
  '';
  buildCommand = "exit 1";
}

