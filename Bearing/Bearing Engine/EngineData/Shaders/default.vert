#version 330 core
in vec3 aPosition;
in vec2 aTexCoord;
in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec3 pos;

void main()
{
    pos = vec3(vec4(aPosition, 1.0f)*model);

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;

    Normal = aNormal*mat3(model);
}