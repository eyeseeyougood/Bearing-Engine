#version 430 core
in vec3 aPosition;
in vec2 aTexCoord;
in vec3 aNormal;

uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec3 pos;
out vec2 texCoord;

layout(std430, binding = 0) readonly buffer ssbo0
{
    mat4 models[];
};

void main()
{
    mat4 model = transpose(models[gl_InstanceID]);

    vec3 localPos = aPosition;

    pos = vec3(vec4(localPos, 1.0f)*model);

    gl_Position = vec4(localPos, 1.0) * model * view * projection;

    Normal = aNormal*mat3(model);
    
    texCoord = aTexCoord;
}