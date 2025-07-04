#version 330 core
in vec2 aPosition;
in vec2 aTexCoord;

uniform vec2 posOffset;
uniform vec2 posScale;
uniform vec2 sizeOffset;
uniform vec2 sizeScale;

uniform vec2 screenSize;

out vec2 texCoord;

void main()
{
    gl_Position = vec4(aPosition + posScale + posOffset, 1.0, 1.0);
    texCoord = aTexCoord;
}