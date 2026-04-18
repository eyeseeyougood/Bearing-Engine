#version 330 core
in vec3 aPosition;
in vec2 aTexCoord;
in vec3 aNormal;

uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec2 texCoord;

void main()
{
    mat4 rotation = mat4(mat3(view));

    vec4 position = vec4(aPosition, 1.0) * rotation * projection;

    gl_Position = position.xyww;

    Normal = aNormal;
    
    texCoord = aTexCoord;
}