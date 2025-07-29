#version 330 core
in vec2 aPosition;
in vec2 aTexCoord;

uniform vec2 anchor;
uniform vec2 posOffset;
uniform vec2 posScale;
uniform vec2 sizeOffset;
uniform vec2 sizeScale;

uniform vec2 texSize;

uniform vec2 screenSize;

uniform int fitToTexRatio;

out vec2 texCoord;

void main()
{
    vec2 sizing = sizeScale + (sizeOffset/screenSize);

    vec2 anchorOffset = sizing * anchor;

    vec2 positioning = posScale + (posOffset/screenSize)
                     - ((vec2(1,1) - sizing) / 2)
                     - anchorOffset;

    positioning = vec2(positioning.x, -positioning.y);

    gl_Position = vec4(aPosition * sizing + positioning, 0.0, 0.5);

    if (fitToTexRatio == 0)
    {
        texCoord = aTexCoord;
        return;
    }

    // adjust UVs to fit to text to confines
    vec2 quadPixels = sizing * screenSize;

    float quadAspect = quadPixels.x / quadPixels.y;
    float texAspect = texSize.x / texSize.y;

    float scaleX = 1.0;
    float scaleY = texAspect / quadAspect;

    if (scaleY < 1.0)
    {
        scaleY = 1.0;
        scaleX = quadAspect / texAspect;
    }

    vec2 texCenter = aTexCoord - vec2(0.5);
    texCenter *= vec2(scaleX, scaleY);
    texCoord = texCenter + vec2(0.5);
}