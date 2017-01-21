#include <bits/stdc++.h>
#include <GL/glut.h>

using namespace std;
double cx=0,cy=0,cz=0;
double crsx, crsy;

struct Point
{
    char c;
    double x, y;
    Point(char cc, double xx, double yy): c(cc), x(xx), y(yy) {}
};

vector <Point> seq;

void theCube()
{
    glPushMatrix();
    glColor3f(1,1,1);
    glTranslatef(cx,cy,cz);
    glutSolidCube(0.4);
    glPopMatrix();
}

void drawGrid()
{
    int i;
    for(i=0;i<40;i++)
    {
        glPushMatrix();
        if(i<20)
            glTranslatef(0,0,i);
        if(i>=20)
        {
             glTranslatef(i-20,0,0);
             glRotatef(-90,0,1,0);
        }
        glBegin(GL_LINES);
        glColor3f(1,1,1);
        glLineWidth(1);
        glVertex3f(0,-0.1,0);
        glVertex3f(19,-0.1,0);
        glEnd();
        glPopMatrix();
    }
}

void drawgg()
{

        glColor3f(1,1,1);
    for(double i=-10.0; i<=10.0; i++)
    {
        glBegin(GL_LINES);
        glLineWidth(1);
        glVertex2f(i,-10.0);
        glVertex2f(i,10.0);
        glEnd();
    }

    for(double i=-10.0; i<=10.0; i++)
    {
        glBegin(GL_LINES);
        glLineWidth(1);
        glVertex2f(-10.0,i);
        glVertex2f(10.0,i);
        glEnd();
    }
}

void drawsq()
{
    glPushMatrix();
    glTranslatef(cx,cy,cz);
    glBegin(GL_QUADS);
    glVertex2f(-10, 10);
    glVertex2f(-9, 10);
    glVertex2f(-9, 9);
    glVertex2f(-10, 9);
    glEnd();
    glPopMatrix();
}

void drawg()
{

    glColor3f(1,1,1);

    /// x axis
    glBegin(GL_LINES);
    glLineWidth(1);
    glVertex2f(-500.0, 0.0);
    glVertex2f(500.0, 0.0);
    glEnd();

    /// y axis
    glBegin(GL_LINES);
    glLineWidth(1);
    glVertex2f(0.0, -500.0);
    glVertex2f(0.0, 500.0);
    glEnd();



}

void draws(double x, double y)
{
//    glPushMatrix();
//    glTranslatef(cx,cy,cz);
    glBegin(GL_QUADS);
    glVertex2f(x-5.0, y+5.0);
    glVertex2f(x+5.0, y+5.0);
    glVertex2f(x+5.0, y-5.0);
    glVertex2f(x-5.0, y-5.0);
    glEnd();
//    glPopMatrix();
}

void drawX(double x, double y)
{
    glBegin(GL_LINES);
    glLineWidth(1);

    glVertex2f(crsx, crsy);
    glVertex2f(x-5.0, y+5.0);

    glVertex2f(crsx, crsy);
    glVertex2f(x+5.0, y+5.0);

    glVertex2f(crsx, crsy);
    glVertex2f(x+5.0, y-5.0);

    glVertex2f(crsx, crsy);
    glVertex2f(x-5.0, y-5.0);

    glEnd();
}


void init()
{
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    gluPerspective(35,1.0f,0.1f,1000);
    glMatrixMode(GL_MODELVIEW);
    glEnable(GL_DEPTH_TEST);
    glClearColor(0.1,0.1,0.1,1);
}

void drawSequence(int i)
{
    if(i == seq.size())
    {
        seq.clear();
        return;
    }
    if(seq[i].c == 'p')
    {
        crsx = seq[i].x;
        crsy = seq[i].y;
    }
    else
    {
        cx = seq[i].x;
        cy = seq[i].y;
    }

    glutPostRedisplay();
    glutTimerFunc(200, drawSequence, i+1);
}


void readF()
{
    string s;
    double x, y;
    ifstream iFile("C:\\Users\\Abid\\Desktop\\Simulator\\in.txt");    // input.txt has integers, one per line

    while (true)
    {
        if( iFile.eof() ) break;
        iFile >> s;
        iFile >> x;
        iFile >> y;

        seq.push_back(Point(s[0], x, y));
    }

    drawSequence(0);
}

void keyboard(unsigned char key, int x, int y)
{
    if(key=='w') cy += 1;
    if(key=='a') cx -= 1;
    if(key=='s') cy -=1;
    if(key=='d') cx += 1;
    if(key == 'g') readF();
    glutPostRedisplay();
}

void display()
{
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glLoadIdentity();
    glTranslatef(0,0,-500);
//    glRotatef(45,1,1,0);

    draws(cx, cy);
    drawg();
    drawX(crsx, crsy);
//    drawGrid();
//    theCube();
    glutSwapBuffers();
}

int main(int argc, char **argv)
{
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE);
    glutInitWindowSize(800,600);
    glutCreateWindow("Robot Navigation");
    init();
    glutKeyboardFunc(keyboard);
    glutDisplayFunc(display);
    glutMainLoop();
    return 0;
}
