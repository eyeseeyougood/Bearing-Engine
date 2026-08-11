#version 330 core

in vec3 aPosition;
in vec2 aTexCoord;
in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 cameraBackDirection;
uniform float lineWidth;

#define NO_POINTS 100

struct Point
{
    vec3 pos;
};

uniform Point points[NO_POINTS];
uniform int numPoints;

out vec3 pos;
out vec2 texCoord;

void main()
{
    float left = aNormal.y;

    int pIndex = int(aNormal.x);

    vec3 point = points[pIndex].pos;

    vec3 c;

    if (pIndex == 0)
    {
        vec3 diff = normalize(points[1].pos - point);
        c = normalize(cross(normalize(cameraBackDirection), diff));
    }
    else if (pIndex == numPoints - 1)
    {
        vec3 diff = normalize(point - points[pIndex - 1].pos);
        c = normalize(cross(normalize(cameraBackDirection), diff));
    }
    else
    {
        vec3 prevDiff = normalize(point - points[pIndex - 1].pos);
        vec3 nextDiff = normalize(points[pIndex + 1].pos - point);

        vec3 prevC = normalize(cross(normalize(cameraBackDirection), prevDiff));
        vec3 nextC = normalize(cross(normalize(cameraBackDirection), nextDiff));

        vec3 joint = normalize(prevC + nextC);

        float jointLength = 1.0 / max(dot(joint, nextC), 0.001);
        jointLength = min(jointLength, 4.0);

        c = joint * jointLength;
    }

    vec3 worldPos = vec3(vec4(point, 1.0) * model);
    worldPos += left * c * lineWidth;

    pos = worldPos;
    texCoord = aTexCoord;

    gl_Position = vec4(worldPos, 1.0) * view * projection;
}