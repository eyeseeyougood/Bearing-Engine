#version 330 core

in vec2 texCoord;

out vec4 FragColor;

uniform sampler2D sceneColor;

float LinearizeDepth(float depth)
{
    float nearPlane = 0.01;
    float farPlane = 100;
    float z = depth * 2.0 - 1.0;
    return (2.0 * nearPlane * farPlane) / (farPlane + nearPlane - z * (farPlane - nearPlane));
}

void main()
{
    vec4 color = texture(sceneColor, vec2(1,-1)*texCoord);
    
    FragColor = color;
}