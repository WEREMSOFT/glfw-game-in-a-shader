#define MAX_STEPS 100
#define MAX_DIST 100.
#define MIN_DIST .01

#define TAU 6.283185
#define PI 3.141592

#define MAT_DEFAULT 0
#define MAT_CHECKERS 1
#define MAT_MIRROR 2
#define MAT_GOLD 3


struct Camera
{
    vec3 position;
    vec3 direction;
    vec3 lookAt;
};

float getDist2Box(vec3 p, vec3 size)
{
    return length(max(abs(p)-size, 0.));
}

float getDist2Sphere(vec3 p, float radius)
{
    return length(p) - radius;
}

float getDistance(vec3 p)
{
    float floorD = p.y + 4.;
    // distance to sphere
    vec3 sphere2 = vec3(1., 1., 2.5);
    float ds2 = getDist2Sphere(p - sphere2, 1.);

    // distance to sphere
    vec3 sphere = vec3(1., 1., 0.);
    float ds = getDist2Sphere(p - sphere, 1.);

    float boxDistance = getDist2Box(p-vec3(-1.5,.75, .5), vec3(.75));

    return min(min(min(ds, floorD), boxDistance), ds2);
}

int getMaterial(vec3 p)
{
    float floorD = p.y + 4.;

  // distance to sphere2
    vec3 sphere2 = vec3(1., 1., 2.5);
    float ds2 = getDist2Sphere(p - sphere2, 1.);

    // distance to sphere
    vec3 sphere = vec3(1., 1., 0.);
    float ds = getDist2Sphere(p - sphere, 1.);

    float boxDistance = getDist2Box(p-vec3(-1.5,.75, .5), vec3(.75));

    float d = min(min(min(ds, floorD), boxDistance), ds2);
    
    if(d == floorD)
        return MAT_CHECKERS;
        
    if(d == ds || d == ds2)
        return MAT_MIRROR;
        
    return MAT_DEFAULT;
}

// Stable softshadow function from https://iquilezles.org/articles/rmshadows/
float softshadow( in vec3 ro, in vec3 rd, float mint, float maxt, float k )
{
    float res = 1.0;
    float ph = 1e20;
    for( float t=mint; t<maxt; )
    {
        float h = getDistance(ro + rd*t);
        if( h<0.001 )
            return 0.0;
        float y = h*h/(2.0*ph);
        float d = sqrt(h*h-y*y);
        res = min( res, k*d/max(0.0,t-y) );
        ph = h;
        t += h;
    }
    return res;
}

vec3 getNormal(vec3 p)
{
    vec2 e = vec2(0.01, 0.);
    
    float d = getDistance(p);
    
    vec3 n = d - vec3(
        getDistance(p - e.xyy), 
        getDistance(p - e.yxy),
        getDistance(p - e.yyx));    
    
    return normalize(n);
}

float rayMarch(vec3 ro, vec3 rd)
{
    
    float dO = 0.;
    
    for(int i = 0; i < MAX_STEPS; i++)
    {
        float dl = getDistance(ro + rd * dO);
        dO += dl;
        if(dl < MIN_DIST || dO > MAX_DIST) break;
    }

    return dO;
}

float getLight(vec3 p)
{
    vec3 lightPosition = vec3(-1., 5., 6.);
    
    float lightMoveRadius = 7.;
    
    lightPosition.x = sin(iTime) * lightMoveRadius;
    lightPosition.z = cos(iTime) * lightMoveRadius;
    
    vec3 lightDirection = normalize(lightPosition - p);
    
    vec3 n = getNormal(p);
    
    float angleNormLight = dot(lightDirection, n);
    
    float distanceToLight = rayMarch(p + n * .1, lightDirection);
    
    float shadow = softshadow(p, lightDirection, 0.01, 100., 8.);
    
    return angleNormLight * shadow;
}

float checkers( in vec3 p )
{
    vec3 s = sign(fract(p*.5)-.5);
    return .5 - .5*s.x*s.z;
}

mat2 getRotationMatrix(float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

vec3 GetRayDir(vec2 uv, vec3 p, vec3 l, float z)
{
    vec3 f = normalize(l - p),
         r = normalize(cross(vec3(0, 1, 0), f)),
         u = cross(f, r),
         c = f * z,
         i = c + uv.x * r + uv.y * u,
         d = normalize(i);
    return d;
}


Camera getCamera(vec2 screenPosition)
{
    vec2 mousePosition = iMouse.xy / iResolution.xy;

    Camera camera;
    camera.position = vec3(0, 1., -10.);
    camera.lookAt = vec3(0, 0, 0);

    camera.position.yz *= getRotationMatrix(-mousePosition.y * PI + 1.);
    camera.position.xz *= getRotationMatrix(-mousePosition.x * TAU);
    camera.direction = GetRayDir(screenPosition, camera.position, camera.lookAt, 1.);
    return camera;
}

vec4 render(inout vec3 ro, inout vec3 rd, inout float reflectivity)
{
    
    float d = rayMarch(ro, rd);

    vec4 color = texture(iChannel4, rd);

    vec3 p = ro + rd * d;
    vec3 n = getNormal(p);

    vec3 texCol = vec3(checkers(p));
    vec3 col = vec3(getLight(p));

    vec3 reflection = reflect(rd, n);
    ro = p + n * MIN_DIST * 3.;
    rd = reflection;

    reflectivity = 0.;

    if(d < MAX_DIST)
    {
        int material = getMaterial(p);

        if(material == MAT_DEFAULT)
        {
            color = vec4(col, 1.0);
            reflectivity = 0.;
        } else if(material == MAT_CHECKERS)
        {
            color = vec4(mix(col, texCol, .5), 1.0);
            reflectivity = .5;
        } else if(material == MAT_MIRROR)
        {
           
            color = vec4(0.);
            reflectivity = .9;
        }

        return color;
    }
    
    return color;
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = (fragCoord-.5 * iResolution.xy) / iResolution.y;

    Camera camera = getCamera(uv);
    
    vec3 ro = camera.position; 
    vec3 rd = camera.direction;
    float reflectivity = 0.;
    vec4 difuse = render(ro, rd, reflectivity);
    vec4 filt = vec4(1.);
    
    const int MAX_BOUNCES = 3;
    
    vec4 reflection = vec4(0.);
    
    for(int i = 0; i < MAX_BOUNCES; i++){
        filt *= reflectivity;
        vec4 bounce = filt * render(ro, rd, reflectivity);
        difuse += bounce; 
    }
    
    fragColor = difuse;
}