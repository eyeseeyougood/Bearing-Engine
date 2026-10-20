#version 330 core
in vec2 aPosition;
in vec2 aTexCoord;

uniform vec2 anchor;
uniform vec2 posOffset;
uniform vec2 posScale;
uniform vec2 sizeOffset;
uniform vec2 sizeScale;

uniform float rotation;

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

    mat2 rotMat = mat2(cos(rotation), -sin(rotation), sin(rotation), cos(rotation));

    vec2 rotatedScaledPosition = rotMat * (aPosition*sizing);

    positioning = vec2(positioning.x, -positioning.y);

    gl_Position = vec4(rotatedScaledPosition + positioning, 0.0, 0.5);

    texCoord = aTexCoord;
}