# Makefile for FNA ABI Compatibility Files
# Written by Ethan "flibitijibibo" Lee

all: Microsoft.Xna.Framework Game Graphics GamerServices Graphics Input.Touch Net Storage Video Xact

Microsoft.Xna.Framework: fnacopy
	mcs Microsoft.Xna.Framework.cs -target:library -r:FNA.dll

Game: fnacopy
	mcs Microsoft.Xna.Framework.Game.cs -target:library -r:FNA.dll -r:FNA.NetStub.dll

GamerServices: fnacopy
	mcs Microsoft.Xna.Framework.GamerServices.cs -target:library -r:FNA.dll -r:FNA.NetStub.dll

Graphics: fnacopy
	mcs Microsoft.Xna.Framework.Graphics.cs -target:library -r:FNA.dll

Input.Touch: fnacopy
	mcs Microsoft.Xna.Framework.Input.Touch.cs -target:library -r:FNA.dll

Net: fnacopy
	mcs Microsoft.Xna.Framework.Net.cs -target:library -r:FNA.dll -r:FNA.NetStub.dll

Storage: fnacopy
	mcs Microsoft.Xna.Framework.Storage.cs -target:library -r:FNA.dll

Video: fnacopy
	mcs Microsoft.Xna.Framework.Video.cs -target:library -r:FNA.dll

Xact: fnacopy
	mcs Microsoft.Xna.Framework.Xact.cs -target:library -r:FNA.dll

fnacopy: clean
	cp ../bin/Debug/FNA.dll .
	cp ../../FNA.NetStub/bin/Debug/FNA.NetStub.dll .

clean:
	rm -f *.dll
