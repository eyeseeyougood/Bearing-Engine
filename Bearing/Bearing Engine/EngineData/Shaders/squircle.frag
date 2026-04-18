#version 330 core
out vec4 FragColor;

in vec2 texCoord;
in float texAspect;

uniform float radius;
uniform vec4 mainColour;

uniform sampler2D texture0;

void main()
{
    vec2 v = (texCoord - vec2(0.5, 0.5));
    vec2 unscaled = vec2(v.x, v.y / 2);
    float dist = length(unscaled);

    if (dist > radius)
        discard;

    vec4 result = mainColour * texture(texture0, texCoord);

    if (result.a < 0.001)
        discard;

    FragColor = result;
}