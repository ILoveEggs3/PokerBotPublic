// GFS -- a pseudo file system for the GPU.
// This file system is for the GPU BCL meta system. There are
// only a few functions available, on it's rather limited in scope.
//
// The file system is:
// * completely flat. There are no directories.
// * completely in memory.
// * an association of a name (null-terminated char string) with
//      a byte array of a given length.
//
// Operations:
// Gfs_init()
// Gfs_add_file()
// Gfs_remove_file()
// Gfs_open_file()
// Gfs_close_file()
// Gfs_read()
// Gfs_length()

#include "_BCL_.h"
#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>
#include <string.h>
#include "Gstring.h"
#include "Filesystem.h"
#include "Sys.h"

//function_space_specifier static char** names;
//function_space_specifier static char** files;
//function_space_specifier static size_t* lengths;
//function_space_specifier static boolean init;
//function_space_specifier static int initial_size;


function_space_specifier void CommonInitFileSystem()
{
    _bcl_->initial_size = 700;
    _bcl_->names = (char**)Gmalloc(_bcl_->initial_size * sizeof(char*));
    memset(_bcl_->names, 0, _bcl_->initial_size * sizeof(char*));
    _bcl_->files = (char**)Gmalloc(_bcl_->initial_size * sizeof(char*));
    memset(_bcl_->files, 0, _bcl_->initial_size * sizeof(char*));
    _bcl_->lengths = (size_t*)Gmalloc(_bcl_->initial_size * sizeof(size_t));
    memset(_bcl_->lengths, 0, _bcl_->initial_size * sizeof(size_t));
    _bcl_->init = 1;
}

function_space_specifier void InternalInitFileSystem()
{
    CommonInitFileSystem();
}

__global__ void Bcl_Gfs_init()
{
    CommonInitFileSystem();
}

function_space_specifier void InternalGfsAddFile(void * name, void * file, size_t length, void * result)
{
    Gfs_add_file((char*)name, (char*)file, length, (int*)result);
}

__global__ void Bcl_Gfs_add_file(char * name, char * file, size_t length, int * result)
{
    Gfs_add_file(name, file, length, result);
}

function_space_specifier void Gfs_add_file(char * name, char * file, size_t length, int * result)
{
    if (_bcl_->init == 0) CommonInitFileSystem();
    char ** ptr_name = _bcl_->names;
    char ** ptr_file = _bcl_->files;
    size_t * ptr_length = _bcl_->lengths;
    for (int i = 0; i < _bcl_->initial_size; ++i)
    {
        if (*ptr_name == NULL)
        {
            //Gprintf("Entered\n");
            *ptr_name = Gstrdup(name);
            *ptr_file = (char *)Gmalloc(length);
            memcpy(*ptr_file, file, length);
            *ptr_length = length;
            //*result = i;
            return;
        }
        else
        {
            ptr_name++;
            ptr_file++;
            ptr_length++;
        }
    }
    Crash("File system overflow. Cannot handle more than %d files.\n", _bcl_->initial_size);
    //Gprintf("Not entered\n");
    //*result = -1;
}

function_space_specifier void Gfs_add_file_no_malloc(const char * name, const char * file, size_t length)
{
    if (_bcl_->init == 0) CommonInitFileSystem();
    char ** ptr_name = _bcl_->names;
    char ** ptr_file = _bcl_->files;
    size_t * ptr_length = _bcl_->lengths;
    // First search for file. No duplicates.
    for (int i = 0; i < _bcl_->initial_size; ++i)
    {
        if (*ptr_name == NULL)
            break;
        if (Gstrcmp(*ptr_name, name) == 0)
            return;
    }
    for (int i = 0; i < _bcl_->initial_size; ++i)
    {
        if (*ptr_name == NULL)
        {
            *ptr_name = Gstrdup(name);
            *ptr_file = (char*)file;
            *ptr_length = length;
            return;
        }
        else
        {
            ptr_name++;
            ptr_file++;
            ptr_length++;
        }
    }
    Crash("File system overflow. Cannot handle more than %d files.\n", _bcl_->initial_size);
}

__global__ void Bcl_Gfs_remove_file(char * name, int * result)
{
    Gfs_remove_file(name, result);
}

function_space_specifier void Gfs_remove_file(char * name, int * result)
{
    // Delete a pseudo file system for the meta system to work,
    if (_bcl_->init == 0) CommonInitFileSystem();
    char ** ptr_name = _bcl_->names;
    char ** ptr_file = _bcl_->files;
    size_t * ptr_length = _bcl_->lengths;
    // First search for file. No duplicates.
    for (int i = 0; i < _bcl_->initial_size; ++i)
    {
        if (*ptr_name == NULL)
            break;
        if (Gstrcmp(*ptr_name, name) == 0)
            return;
    }
    for (int i = 0; i < _bcl_->initial_size; ++i)
    {
        if (*ptr_name != NULL && Gstrcmp(*ptr_name, name) == 0)
        {
            Gfree(*ptr_name);
            *ptr_name = NULL;
            Gfree(*ptr_file);
            *ptr_file = NULL;
            *ptr_length = 0;
            *result = i;
            return;
        }
        else
        {
            ptr_name++;
            ptr_file++;
            ptr_length++;
        }
    }
    *result = -1;
}

__global__ void Bcl_Gfs_open_file(char * name, int * result)
{
    Gfs_open_file(name, result);
}

function_space_specifier void Gfs_open_file(char * name, int * result)
{
    if (_bcl_->init == 0) CommonInitFileSystem();
    char ** ptr_name = _bcl_->names;
    char ** ptr_file = _bcl_->files;
    size_t * ptr_length = _bcl_->lengths;
    for (int i = 0; i < _bcl_->initial_size; ++i)
    {
        if (*ptr_name != NULL && Gstrcmp(*ptr_name, name) == 0)
        {
            *result = i;
            return;
        }
        else
        {
            ptr_name++;
            ptr_file++;
            ptr_length++;
        }
    }
    *result = -1;
}

__global__ void Bcl_Gfs_close_file(int file, int * result)
{
    Gfs_close_file(file, result);
}

function_space_specifier void Gfs_close_file(int file, int * result)
{
    if (_bcl_->init == 0) CommonInitFileSystem();
    *result = 0;
}

__global__ void Bcl_Gfs_read(int file, char ** result)
{
    Gfs_read(file, result);
}

function_space_specifier void Gfs_read(int file, char ** result)
{
    if (_bcl_->init == 0) CommonInitFileSystem();
    if (file >= 0)
        *result = _bcl_->files[file];
    else
        *result = 0;
}

__global__ void Bcl_Gfs_length(int file, size_t * result)
{
    Gfs_length(file, result);
}

function_space_specifier void Gfs_length(int file, size_t * result)
{
    if (_bcl_->init == 0) CommonInitFileSystem();
    *result = _bcl_->lengths[file];
}

