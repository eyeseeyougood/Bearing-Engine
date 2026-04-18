#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec4 mainColour;

uniform sampler2D texture0;

void main()
{
    FragColor = mainColour * texture(texture0, texCoord);
}