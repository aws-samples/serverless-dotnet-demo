#LT_NET_SDK_CHANNEL
#Specifies the source channel for the installation. The possible values are:
#    STS: The most recent Standard Term Support release.
#    LTS: The most recent Long Term Support release.
#    Two-part version in A.B format, representing a specific release (for example, 3.1 or 6.0).
#    Three-part version in A.B.Cxx format, representing a specific SDK release (for example, 6.0.1xx or 6.0.2xx). Available since the 5.0 release.
echo --------------------------------------------
echo Installing dotnet runtime version: $LT_NET_SDK_CHANNEL
echo --------------------------------------------

curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel $LT_NET_SDK_CHANNEL

echo --------------------------------------------
echo Installing artillery
echo --------------------------------------------
npm i artillery -g