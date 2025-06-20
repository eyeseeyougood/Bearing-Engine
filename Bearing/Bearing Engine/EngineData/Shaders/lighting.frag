#version 330 core
out vec4 FragColor;

uniform vec4 mainColour;

in vec3 Normal;
in vec3 pos;

uniform vec3 cameraPos;

struct PointLight {
    vec3 pos;
    vec4 col;
};

#define NO_POINT_LIGHTS 100

uniform PointLight pointLights[NO_POINT_LIGHTS];
uniform int numPointLights;

void main()
{
    vec4 final = vec4(0,0,0,0);
    for (int i = 0; i < numPointLights; i++)
    {
        vec3 lightPos = pointLights[i].pos;
        vec4 lightColor = pointLights[i].col;

        float ambient = 0.2f;

        vec3 normal = normalize(Normal);
        vec3 lightDirection = normalize(lightPos - pos);

        float diffuse = max(dot(normal, lightDirection), 0.0f);

        float specularLight = 0.5f;

        float specular = 0.0;
        if (diffuse != 0.0)
        {
            vec3 viewDirection = normalize(cameraPos - pos);

            vec3 halfwayVec = normalize(viewDirection + lightDirection);

            float specAmount = pow(max(dot(normal, halfwayVec), 0.0f), 16);
            specular = specAmount * specularLight;
        }

        final = final + vec4(lightColor.xyz*(diffuse+ambient+specular), 1.0);
    }
    final = final / numPointLights;

    final = vec4(final.xyz, 1.0);

    FragColor = mainColour * final;
}