#include <stdio.h>
#include <stdlib.h>
#include <GLFW/glfw3.h>
#include <iostream>

#include "glFunctionLoader.hpp"

#define SCREEN_WIDTH 800
#define SCREEN_HEIGHT 600

class Program
{
    GLFWwindow *window;
    unsigned int shaderProgram;
    unsigned int VBO, VAO, EBO;

public:
    Program(void)
    {
        if (!glfwInit())
            exit(-1);

        glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
        glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
        glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

        window = glfwCreateWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "HEllo World!!", NULL, NULL);

        if (!window)
        {
            glfwTerminate();
            exit(-1);
        }

        glfwMakeContextCurrent(window);
        loadOpenGLFunctions();
        glClearColor(1.f, 0, 0, 1.f);

        // build and compile our shader program
        // ------------------------------------
        shaderProgram = shaderProgramCreateFromFilesShadertoy("assets/shader.vs", "assets/shader.fs");
    }

    void runMailLoop(void)
    {
        float vertices[] = {
            1.0f, 1.0f, 0.0f, 1.0f, 1.0f,   // top right
            1.0f, -1.0f, 0.0f, 1.0f, 0.0f,  // bottom right
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom left
            -1.0f, 1.0f, 0.0f, 0.0f, 1.0f   // top left
        };
        unsigned int indices[] = {
            0, 1, 3, // first triangle
            1, 2, 3  // second triangle
        };

        glGenVertexArrays(1, &VAO);
        glGenBuffers(1, &VBO);
        glGenBuffers(1, &EBO);
        // bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
        glBindVertexArray(VAO);

        glBindBuffer(GL_ARRAY_BUFFER, VBO);
        glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
        glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

        glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 5 * sizeof(float), (void *)0);
        glEnableVertexAttribArray(0);
        glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, 5 * sizeof(float), (void *)(3 * sizeof(float)));
        glEnableVertexAttribArray(1);

        char *uniformName = "screenSize";

        // note that this is allowed, the call to glVertexAttribPointer registered VBO as the vertex attribute's bound vertex buffer object so afterwards we can safely unbind
        glBindBuffer(GL_ARRAY_BUFFER, 0);

        // remember: do NOT unbind the EBO while a VAO is active as the bound element buffer object IS stored in the VAO; keep the EBO bound.
        // glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);

        // You can unbind the VAO afterwards so other VAO calls won't accidentally modify this VAO, but this rarely happens. Modifying other
        // VAOs requires a call to glBindVertexArray anyways so we generally don't unbind VAOs (nor VBOs) when it's not directly necessary.
        glBindVertexArray(0);
        int frame = 0;
        while (!glfwWindowShouldClose(window))
        {
            frame++;
            // render
            // ------
            glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            glClear(GL_COLOR_BUFFER_BIT);

            // draw our first triangle
            glUseProgram(shaderProgram);
            GLint uniformScreenSizeLocation = glGetUniformLocation(shaderProgram, "iResolution");
            glUniform3f(uniformScreenSizeLocation, SCREEN_WIDTH, SCREEN_HEIGHT, 1.0);

            GLint timeUniformLocation = glGetUniformLocation(shaderProgram, "iTime");
            glUniform1f(timeUniformLocation, glfwGetTime());

            double xpos, ypos;
            glfwGetCursorPos(window, &xpos, &ypos);

            GLint frameUniformLocation = glGetUniformLocation(shaderProgram, "iFrame");
            glUniform1i(frameUniformLocation, frame);

            GLint mouseUniformLocation = glGetUniformLocation(shaderProgram, "iMouse");
            glUniform4f(mouseUniformLocation, xpos, ypos, 0, 0);

            glBindVertexArray(VAO); // seeing as we only have a single VAO there's no need to bind it every time, but we'll do so to keep things a bit more organized
            // glDrawArrays(GL_TRIANGLES, 0, 6);
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);
            // glBindVertexArray(0); // no need to unbind it every time

            // glfw: swap buffers and poll IO events (keys pressed/released, mouse moved etc.)
            // -------------------------------------------------------------------------------
            glfwSwapBuffers(window);
            glfwPollEvents();
        }
    }

    ~Program()
    {
        glDeleteVertexArrays(1, &VAO);
        glDeleteBuffers(1, &VBO);
        glDeleteBuffers(1, &EBO);
        glDeleteProgram(shaderProgram);
        glfwTerminate();
    }
};