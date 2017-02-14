#include <bits/stdc++.h>
#include <GL/glut.h>

using namespace std;
double cx=0,cy=0,cz=0;
double crsx = 50.0, crsy=-10.0;
double dsx=0.0, dsy=0.0, dex=0.0, dey=50.0;
double ux = 0.0, uy = -10.0;
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
    glColor3f(0.0, 1.0, 0.0);
    glBegin(GL_QUADS);
    glVertex2f(x-5.0, y+5.0);
    glVertex2f(x+5.0, y+5.0);
    glVertex2f(x+5.0, y-5.0);
    glVertex2f(x-5.0, y-5.0);
    glEnd();
//    glPopMatrix();
}

void drawu(double x, double y)
{
//    glPushMatrix();
//    glTranslatef(cx,cy,cz);
    glColor3f(1.0, 0.0, 0.0);
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
    glColor3f(1.0, 1.0, 1.0);

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
    if(seq[i].c == 's')
    {
        dsx = seq[i].x;
        dsy = seq[i].y;
        dex = seq[i+1].x;
        dey = seq[i+1].y;

        ux = seq[i].x;
        uy = seq[i].y;

        glutPostRedisplay();
        glutTimerFunc(5, drawSequence, i+2);
        return;
    }
    else if(seq[i].c == 'r')
    {
        cx = seq[i].x;
        cy = seq[i].y;
    }
    else
    {
        crsx = seq[i].x;
        crsy = seq[i].y;
    }

    glutPostRedisplay();
    glutTimerFunc(5, drawSequence, i+1);
}


void readF(int a)
{
    string s;
    double x, y;
    double px, py;
    ifstream ifs("data.txt");    // input.txt has integers, one per line
    if(!ifs) puts("ERROR");

//    glutTimerFunc(5, drawSequence, 0);
    if (ifs.is_open())
    {
        string line;
        while (true)
        {
            while (getline(ifs, line))
            {
                stringstream ss(line);
                ss >> s;
                ss >> x;
                ss >> y;

                y = -y;
//                cout << s << " " << x << " " << y << endl;
                Sleep(5);
                if(s[0] == 's')
                {
                    px = x;
                    py = y;
                }
                else if(s[0] == 'p')
                {
                    dsx = px;
                    dsy = py;
                    dex = x;
                    dey = y;

                    ux = px;
                    uy = py;

                }
                else if(s[0] == 'r')
                {
                    cx = x;
                    cy = y;
                }
                else
                {
                    crsx = x;
                    crsy = y;
                }

            }
            if (!ifs.eof()) break; // Ensure end of read was EOF.
            ifs.clear();

            // You may want a sleep in here to avoid
            // being a CPU hog.
        }
    }

//    drawSequence(0);
}

void keyboard(unsigned char key, int x, int y)
{
    if(key=='w') cy += 1;
    if(key=='a') cx -= 1;
    if(key=='s') cy -=1;
    if(key=='d') cx += 1;
//    if(key == 'g') readF();
    glutPostRedisplay();
}

void drawDirection()
{
    glBegin(GL_LINES);
    glColor3f(1.0, 0.0, 0.0);

    glVertex2f(dsx, dsy);
    glVertex2f(dex, dey);

    glEnd();
}

void display()
{
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glLoadIdentity();
    glTranslatef(0,0,-850);
    glScalef(0.5, 0.5, 0.5);
//    glRotatef(45,1,1,0);

    draws(cx, cy);
    drawu(ux, uy);
    drawg();
    drawDirection();
    drawX(crsx, crsy);
//    drawGrid();
//    theCube();
    glutSwapBuffers();
}

int main(int argc, char **argv)
{
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_DOUBLE);
    glutInitWindowSize(700,700);
    glutCreateWindow("Robot Navigation");
    init();
    glutKeyboardFunc(keyboard);
    glutDisplayFunc(display);
    glutIdleFunc(display);
    thread t(readF, 0);
    glutMainLoop();
    return 0;
}
