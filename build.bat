
@echo off
mkdir bin
pushd src
cl -Zi ^
   /std:c++17 ^
   /I ..\..\libs\glfw\include ^
   /I ..\..\libs\glfw\imgui ^
   main.cpp ^
   ..\libs\imgui\imgui.cpp^
   ..\libs\imgui\imgui_demo.cpp^
   ..\libs\imgui\imgui_draw.cpp^
   ..\libs\imgui\imgui_impl_glfw.cpp^
   ..\libs\imgui\imgui_impl_opengl3.cpp^
   ..\libs\imgui\imgui_tables.cpp^
   ..\libs\imgui\imgui_widgets.cpp^
    ..\..\libs\glfw\lib-vc2022\glfw3dll.lib ^
    ..\..\libs\glfw\lib-vc2022\glfw3_mt.lib ^
    ..\..\libs\glew\lib\Release\x64\glew32.lib ^
    opengl32.lib ^
   -o ../bin/main.exe
popd

xcopy ..\libs\glfw\lib-vc2022\glfw3.dll bin /Y
xcopy assets bin\assets /E /H /C /I /Y

pushd bin
main.exe
popd
