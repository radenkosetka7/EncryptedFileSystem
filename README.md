# EncryptedFileSystem,  Faculty of Electrical Engineering, 2021

Application specification

A simple console application that represents a shared encrypted file system for more
user. The user logs into the system using a valid username and password
digital certificate. After logging in to the system, the user sees the complete directory tree
whose root is the home directory that bears the name of the logged-in user.
The entire content of the file system should be protected, where every user can
it only sees its own directories and files. The following must be supported as a minimum
file types/formats: txt, docx, png, jpeg, pdf. The system should prevent the use
a file whose integrity has been violated.
The registered user has the following functionalities available:
- creation of a new text file, where the user specifies the name and content
file, after which it is saved on the file system,
- opening the file in the appropriate program and displaying the contents,
- upload files from the host file system to the encrypted file system,
- download files from the encrypted file system to the host file system, whereby
the file is decrypted before downloading,
- changing the contents of the (text) file,
- file deletion.
The system also offers an easy way to share files, using a unique share
directory. A user who wants to share a file with another user should leave
file to a shared directory, so that only the intended user can
open and see its contents. Make sure they can see the contents of the shared directory
all users of the system.
