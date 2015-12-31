{
   Copyright 2015 Wes Sleeman

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
}
refresh
type "Hello, and welcome to the WCompiler for the "
forecolor = "red"
type "W Programming Language!"
resetcolors
newline
pause 7
refresh
{
type "Do you like it so far? (y/n)"
item x
read x
type x
if x = '1' [
type "Great!"
]
if x = '1' [
type "It'll grow on you when you try using it!"
]
}
newline
pause 5
reset
type "Why don't you try looking at the source code of this little gizmo? It's located in the same directory as the compiler."
newline
pause 10
type "Good luck and have fun!"
backcolour = "blue"
forecolour = "red"
type "-\/\/"
FORECOLOUR = "GREEN"
TYPE "35"
nEwliNe
resetcolors
reset
?Thanks for reading the code. Happy devving!