:: Debug build
:: If you use this remember to put the .pdb in the Unity plugins folder!
:: cl /LD /Od build.cpp lz4.c user32.lib /Fe:VoxelWorldNative.dll /nologo /W3 /Z7 /link /opt:icf /opt:ref /incremental:no

:: Optimized build
cl /LD /O2 /DNDEBUG build.cpp lz4.c user32.lib /Fe:VoxelWorldNative.dll /nologo /W3 /link /opt:icf /opt:ref /incremental:no