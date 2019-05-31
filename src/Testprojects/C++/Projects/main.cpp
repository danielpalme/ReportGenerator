#include "sub/calculator.cpp"

int GreatestOfThree(int a,int b,int c){
  if((a>b) && (a>c)){
    return a;
   }
   else if(b>c){
    return b;
   }
  else{
    return c;
  }
 return 0;
}


int main()
{
  GreatestOfThree(1,2,3);
  Add(3,1);
  Add(1,3);
 return 0;

}