#!/bin/bash

# -----------------------------------------------------------------------------
# Config
# -----------------------------------------------------------------------------
appCA="ca_appTrusted"
hostCA="ca_hostTrusted"
otherCA="ca_untrusted"

appInt="int_appTrusted"
hostInt1="int_hostTrusted"
hostInt2="int_appTrustsRootNotInt"
hostInt3="int_appTrustsIntNotRoot"
otherInt="int_untrusted"

appClient="client_appTrusted"
hostClient1="client_hostTrusted"
hostClient2="client_appTrustsRootNotInt"
hostClient3="client_appTrustsIntNotRoot"
otherClient="client_untrusted"

goodServer="localhost"
badNameServer="badName"
badChainServer="badChain"

clientCertDir="Client/ClientCerts"
serverCertDir="GrpcAuth/ServerCerts"
appTrustDir="GrpcAuth/AppTrustCerts"
hostTrustDir="InstallThese"

# -----------------------------------------------------------------------------
# Helper Functions
# -----------------------------------------------------------------------------
function GeneratePrivateKey {
  echo "-- Generating key for $1"
  openssl genrsa -out $1.key -passout pass:
}

function GenerateCsr {
  echo "-- Generating CSR for $1"
  openssl req -new -key ${1}.key -out ${1}.csr -subj "/CN=$1"
}

function SignCsr {
  echo "-- $2 signing $1.csr for $3"
  openssl x509 \
    -req -in $1.csr -out $1.cer \
    -CA $2.cer -CAkey $2.key -CAcreateserial \
    -days 3650 -extensions $3
}

function PackFiles {
  openssl pkcs12 -export -in $1.cer -inkey $1.key -out $1.pfx -passout pass:
}

function CreateCA {
  echo "- Creating CA $1"
  openssl req \
    -newkey rsa:4096 -nodes -keyform PEM -keyout $1.key \
    -outform PEM -out $1.cer \
    -x509 -days 3650 -subj "/CN=$1" -extensions v3_ca
}

function CreateIntermediateCA {
  echo "- Creating intermediate CA $1"
  GeneratePrivateKey $1
  GenerateCsr $1
  SignCsr $1 $2 "v3_intermediate_ca"
}

function CreateCert {
  echo "- Creating client $1"
  GeneratePrivateKey $1
  GenerateCsr $1
  SignCsr $1 $2 $3
  PackFiles $1
}

function CopyCerts {
  local dest=$1
  local extension=$2
  shift
  shift
  local files=("$@")

  echo "Moving $extension files to $dest"
  mkdir -p $dest

  for file in "${files[@]}"
  do
    cp $file.$extension $dest
  done
}

function CleanTempFiles {
  rm -f *.cer *.csr *.key *.pfx *.srl
}

# -----------------------------------------------------------------------------
# Script
# -----------------------------------------------------------------------------

CreateCA $appCA
CreateCA $hostCA
CreateCA $otherCA

CreateIntermediateCA $appInt   $appCA
CreateIntermediateCA $hostInt1 $hostCA
CreateIntermediateCA $hostInt2 $appCA
CreateIntermediateCA $hostInt3 $hostCA
CreateIntermediateCA $otherInt $otherCA

CreateCert $goodServer    $otherCA "server" 
mv $goodServer.pfx $badChainServer.pfx
CreateCert $goodServer    $appCA   "server"
CreateCert $badNameServer $appCA   "server"

CreateCert $appClient   $appCA    "client"
CreateCert $hostClient1 $hostInt1 "client"
CreateCert $hostClient2 $hostInt2 "client"
CreateCert $hostClient3 $hostInt3 "client"
CreateCert $otherClient $otherInt "client"

CopyCerts $appTrustDir   "cer" $appCA $appInt $hostInt3
CopyCerts $hostTrustDir  "cer" $appCA $hostCA $hostInt1 $hostInt2 $hostInt3
CopyCerts $serverCertDir "pfx" $goodServer $badNameServer $badChainServer
CopyCerts $clientCertDir "pfx" $appClient $hostClient1 $hostClient2 $hostClient3 $otherClient

CleanTempFiles
