SRC = $(shell find src -name *.cpp) $(shell find libs/imgui -name *.cpp)
OBJ = $(patsubst %.cpp,%.obj,$(SRC))
CC = g++
FLAGS = -g -O4 -std=c++17
LIBS = -lglfw -lGL -ldl
TARGET = bin/main.bin

all: copy_assets $(OBJ)
	$(CC) $(FLAGS) $(OBJ) -o $(TARGET) $(LIBS)

clean:
	rm -rf $(TARGET) $(OBJ)

%.obj: %.cpp
	$(CC) -c $(FLAGS) $^ -o $@

run_main: all
	$(TARGET)

copy_assets:
	cp -rf assets bin
