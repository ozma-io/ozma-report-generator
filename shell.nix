{ pkgs ? import <nixpkgs> {} }:

let
  dotnet = pkgs.dotnet-sdk_7;

  env = pkgs.buildFHSUserEnv {
    name = "document-generation";
    targetPkgs = pkgs: with pkgs; [
      dotnet
      unoconv
      icu
    ];
    extraOutputsToInstall = [ "dev" ];
    profile = ''
      export DOTNET_ROOT=${dotnet}
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

