#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec4 mainColour;

void main()
{
    FragColor = mainColour;
}