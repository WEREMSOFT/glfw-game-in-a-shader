#pragma once

static int shaderCreateFromFile(const char *fileName, unsigned int *vertexShader, int shaderType)
{
    int returnValue = 0;
    long fileSize = 0;
    char *shaderCode = NULL;
    *vertexShader = glCreateShader(shaderType);

    FILE *fp = fopen(fileName, "r");

    if (fp == NULL)
    {
        fprintf(stderr, "Error opening file\n");
        returnValue = -1;
        goto error_handler;
    }

    if (fseek(fp, 0, SEEK_END) != 0)
    {
        perror("Error obtaining the file size ");
        returnValue = -1;
        goto error_handler;
    }

    fileSize = ftell(fp);
    fseek(fp, 0, SEEK_SET);

    shaderCode = (char *)calloc(fileSize + 1, sizeof(char));

    if (shaderCode == NULL)
    {
        returnValue = -1;
        goto error_handler;
    }

    if (fread(shaderCode, sizeof(char), fileSize, fp) == 0)
    {
        printf("Error reading file: %s\n", fileName);
        exit(-1);
    }

    glShaderSource(*vertexShader, 1, (const char **)&shaderCode, NULL);

    glCompileShader(*vertexShader);

    int shaderCompilationSuccess;
    char infoLog[512];

    glGetShaderiv(*vertexShader, GL_COMPILE_STATUS, &shaderCompilationSuccess);

    if (!shaderCompilationSuccess)
    {
        glGetShaderInfoLog(*vertexShader, 512, NULL, infoLog);
        printf("%s::%s : vertex shader compilation failed. Error %s\n", __FILE__, __func__, infoLog);
        returnValue = -1;
        goto error_handler;
    }

error_handler:
    if (shaderCode != NULL)
        free(shaderCode);
    if (fp != NULL)
        fclose(fp);

    return returnValue;
}

unsigned int shaderProgramCreateFromFiles(const char *vertexShaderPath, const char *fragmentShaderPath)
{
    unsigned int vs, fs;

    shaderCreateFromFile(vertexShaderPath, &vs, GL_VERTEX_SHADER);
    shaderCreateFromFile(fragmentShaderPath, &fs, GL_FRAGMENT_SHADER);

    unsigned int shaderProgram = glCreateProgram();
    glAttachShader(shaderProgram, vs);
    glAttachShader(shaderProgram, fs);
    if (vs == 0 || fs == 0)
    {
        fprintf(stderr, "error loading shadfer files\n");
        exit(-1);
    }
    glLinkProgram(shaderProgram);

    int success;

    glGetProgramiv(shaderProgram, GL_LINK_STATUS, &success);
    if (!success)
    {
        char infoLog[512];
        glGetProgramInfoLog(shaderProgram, 512, NULL, infoLog);
        printf("%s::%s - Error linking shader program: %s\n", __FILE__, __func__, infoLog);
        exit(-1);
    }

    glDeleteShader(vs);
    glDeleteShader(fs);
    return shaderProgram;
}

static int createShadertoyFSFromFile(const char *fileName, unsigned int *vertexShader, int shaderType)
{
    std::string shaderHeader = "#version 330 core\n"
                               "out vec4 FragColor;"
                               "in vec2 TexCoord;"
                               "uniform vec3 iResolution;"
                               "uniform float iTime;"
                               "uniform vec4 iMouse;"
                               "uniform sampler2D iChannel0;"
                               "uniform sampler2D iChannel1;"
                               "uniform sampler2D iChannel2;"
                               "uniform sampler2D iChannel3;"
                               "uniform samplerCube iChannel4;"
                               "uniform int iFrame;";

    std::string shaderFooter = "void main(){\n"
                               "mainImage(FragColor, TexCoord * iResolution.xy);\n"
                               "}\n";
    int returnValue = 0;
    long fileSize = 0;
    char *shaderCode = NULL;
    *vertexShader = glCreateShader(shaderType);

    FILE *fp = fopen(fileName, "r");

    if (fp == NULL)
    {
        fprintf(stderr, "Error opening file\n");
        exit(-1);
    }

    if (fseek(fp, 0, SEEK_END) != 0)
    {
        perror("Error obtaining the file size ");
        exit(-1);
    }

    fileSize = ftell(fp);
    fseek(fp, 0, SEEK_SET);

    shaderCode = (char *)calloc(fileSize + 1, sizeof(char));

    if (shaderCode == NULL)
    {
        exit(-1);
    }

    if (fread(shaderCode, sizeof(char), fileSize, fp) == 0)
    {
        printf("Error reading file: %s\n", fileName);
        exit(-1);
    }

    std::string shaderCodeString(shaderCode);

    std::string finalShaderCodeString = shaderHeader + shaderCodeString + shaderFooter;

    char *shaderCoderCString = (char *)(finalShaderCodeString.c_str());

    glShaderSource(*vertexShader, 1, (const char **)&shaderCoderCString, NULL);

    glCompileShader(*vertexShader);

    int shaderCompilationSuccess;
    char infoLog[512];

    glGetShaderiv(*vertexShader, GL_COMPILE_STATUS, &shaderCompilationSuccess);

    if (!shaderCompilationSuccess)
    {
        glGetShaderInfoLog(*vertexShader, 512, NULL, infoLog);
        printf("%s::%s : vertex shader compilation failed. Error %s\n", __FILE__, __func__, infoLog);
        returnValue = -1;
        goto error_handler;
    }

error_handler:
    if (shaderCode != NULL)
        free(shaderCode);
    if (fp != NULL)
        fclose(fp);

    return returnValue;
}

unsigned int shaderProgramCreateFromFilesShadertoy(const char *vertexShaderPath, const char *fragmentShaderPath)
{
    unsigned int vs, fs;

    shaderCreateFromFile(vertexShaderPath, &vs, GL_VERTEX_SHADER);
    createShadertoyFSFromFile(fragmentShaderPath, &fs, GL_FRAGMENT_SHADER);

    unsigned int shaderProgram = glCreateProgram();
    glAttachShader(shaderProgram, vs);
    glAttachShader(shaderProgram, fs);
    if (vs == 0 || fs == 0)
    {
        fprintf(stderr, "error loading shadfer files\n");
        exit(-1);
    }
    glLinkProgram(shaderProgram);

    int success;

    glGetProgramiv(shaderProgram, GL_LINK_STATUS, &success);
    if (!success)
    {
        char infoLog[512];
        glGetProgramInfoLog(shaderProgram, 512, NULL, infoLog);
        printf("%s::%s - Error linking shader program: %s\n", __FILE__, __func__, infoLog);
        exit(-1);
    }

    glDeleteShader(vs);
    glDeleteShader(fs);
    return shaderProgram;
}
