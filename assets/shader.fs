
#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .01

struct Camera
{
    vec3 position;
    vec3 direction;
};

float getDistance(vec3 point)
{
    vec4 sphere = vec4(0, 1., 6., 1.);
    float distanceToSphere = length(point - sphere.xyz) - sphere.w;
    float distanceToPlane = point.y;
    return min(distanceToSphere, distanceToPlane);
}

vec3 getNormal(vec3 point)
{
    vec2 e = vec2(.01, 0);
    float distanceToPoint = getDistance(point);
    vec3 n = vec3(
        distanceToPoint - getDistance(point - e.xyy),
        distanceToPoint - getDistance(point - e.yxy),
        distanceToPoint - getDistance(point - e.yyx));

    return normalize(n);
}

float rayMarch(vec3 rayOrigin, vec3 rayDirection)
{
    float distanceToOrigin = 0;

    for (int i = 0; i < MAX_STEPS; i++)
    {
        vec3 point = rayOrigin + rayDirection * distanceToOrigin;
        float distanceToScene = getDistance(point);
        distanceToOrigin += distanceToScene;
        if (distanceToOrigin > MAX_DIST || distanceToScene < SURF_DIST)
            break;
    }

    return distanceToOrigin;
}

float getLight(vec3 point)
{
#define LIGHT_PATH_RADIOUS 2.
    vec3 lightPosition = vec3(0, 5, 6);
    lightPosition.x = cos(iTime * 2.) * LIGHT_PATH_RADIOUS;
    lightPosition.z = 6 + sin(iTime * 2.) * LIGHT_PATH_RADIOUS;
    vec3 lightPosNormalized = normalize(lightPosition - point);
    vec3 n = getNormal(point);
    float diffuse = clamp(dot(n, lightPosNormalized), 0., 1.);

    float d = rayMarch(point + n * SURF_DIST * 2., lightPosNormalized);

    if (d < length(lightPosition - point))
        diffuse *= .1;

    return diffuse;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
    vec2 uv = (fragCoord - .5 * iResolution.xy) / iResolution.y;

    vec3 light = vec3(0, 0, 5);

    Camera camera;
    camera.position = vec3(0, 1, 0);
    camera.direction = normalize(vec3(uv.xy, 1.));

    float distance = rayMarch(camera.position, camera.direction);

    vec3 collisionPoint = camera.position + camera.direction * distance;

    vec4 col = vec4(vec3(getLight(collisionPoint)), 1.);

    fragColor = col;
}