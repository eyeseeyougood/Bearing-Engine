#version 330 core
in vec2 aPosition;
in vec2 aTexCoord;

uniform vec2 position;
uniform float rot;
uniform vec2 scale;
uniform mat4 view;
uniform mat4 projection;

out vec2 pos;
out vec2 texCoord;

void main()
{
    vec2 newPos = aPosition;
    
    mat2 rotMat = mat2(
        cos(rot), -sin(rot),
        sin(rot), cos(rot)
    );

    newPos = newPos * scale;
    newPos = newPos * rotMat;
    newPos = newPos + position;

    pos = newPos;

    vec3 final = vec3(newPos, 0.0);

    gl_Position = vec4(final, 1.0) * view * projection;
    
    texCoord = aTexCoord;
}