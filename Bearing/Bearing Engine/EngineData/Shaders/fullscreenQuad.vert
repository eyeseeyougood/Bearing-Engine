#version 330 core
in vec2 aPosition;
in vec2 aTexCoord;

out vec2 texCoord;

void main()
{
    gl_Position = vec4(aPosition, 0.0, 0.5);
    
    texCoord = aTexCoord;
}