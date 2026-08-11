#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec4 mainColour;
uniform vec4 outlineColour;
uniform vec2 sizeOffset;
uniform vec2 sizeScale;
uniform vec2 screenSize;

uniform int borderWidth;

void main()
{
    vec2 sizing = sizeScale + (sizeOffset/screenSize);
    vec2 pixSize = sizing * screenSize;

    if (texCoord.x > 1.0 - borderWidth / pixSize.x || texCoord.x < 0.0 + borderWidth / pixSize.x || texCoord.y > 1.0 - borderWidth / pixSize.y || texCoord.y < 0.0 + borderWidth / pixSize.y)
        FragColor = outlineColour;
    else
        FragColor = mainColour;
}