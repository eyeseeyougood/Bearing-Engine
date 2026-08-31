#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec4 mainColour;

uniform sampler2D texture0;

void main()
{
    vec4 result = mainColour * texture(texture0, vec2(texCoord.x, 1.0f - texCoord.y));

    if (result.a < 0.001)
        discard;

    FragColor = result;
}