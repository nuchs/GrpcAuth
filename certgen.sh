#!/bin/bash

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
badServer="unlocalhost"

clientCertDir="Client\\ClientCerts"
serverCertDir="GrpcAuth\\ServerCerts"
appTrustDir="GrpcAuth\\AppTrustCerts"
hostTrustDir="InstallThese"

CreateCA $appCA
CreateCA $hostCA
CreateCA $otherCA

CreateIntermediateCA $appInt   $appCA
CreateIntermediateCA $hostInt1 $hostCA
CreateIntermediateCA $hostInt2 $appCA
CreateIntermediateCA $hostInt3 $hostCA
CreateIntermediateCA $otherInt $otherCA

CreateCert $goodServer $appCA "server"
CreateCert $badServer  $appCA "server"

CreateCert $appClient   $appCA    "client"
CreateCert $hostClient1 $hostInt1 "client"
CreateCert $hostClient2 $hostInt2 "client"
CreateCert $hostClient3 $hostInt3 "client"
CreateCert $otherClient $otherInt "client"

mv $appClient.pfx   $clientCertDir
mv $hostClient1.pfx $clientCertDir
mv $hostClient2.pfx $clientCertDir
mv $hostClient3.pfx $clientCertDir
mv $otherClient.pfx $clientCertDir

mv $goodServer.pfx $serverCertDir
mv $badServer.pfx $serverCertDir

mv $appCA.cer $appTrustStore
mv $appInt.cer $appTrustStore
cp $hostInt2.cer $appTrustStore

mv $hostCA.cer $hostTrustDir
mv $hostInt1.cer $hostTrustDir
mv $hostInt2.cer $hostTrustDir
mv $hostInt3.cer $hostTrustDir

rm -f *.cer *.csr *.key *.pfx
