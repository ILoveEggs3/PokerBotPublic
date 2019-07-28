#pragma once

function_space_specifier int Gstrncasecmp(const char *s1, const char *s2, size_t len);
function_space_specifier int Gstrcasecmp(const char *s1, const char *s2);
function_space_specifier char * Gstrcpy(char * dest, const char *src);
function_space_specifier char * Gstrncpy(char * dest, const char *src, size_t count);
function_space_specifier size_t Gstrlcpy(char *dest, const char *src, size_t size);
function_space_specifier char * Gstrcat(char * dest, const char * src);
function_space_specifier char * Gstrncat(char *dest, const char *src, size_t count);
function_space_specifier int Gstrcmp(const char * cs, const char * ct);
function_space_specifier int Gstrncmp(const char * cs, const char * ct, size_t count);
function_space_specifier char * Gstrchr(const char * s, int c);
function_space_specifier char * Gstrrchr(const char * s, int c);
function_space_specifier size_t Gstrlen(const char * s);
function_space_specifier size_t Gstrnlen(const char * s, size_t count);
function_space_specifier char * Gstrdup(const char *s);
function_space_specifier size_t Gstrspn(const char *s, const char *accept);
function_space_specifier char * Gstrpbrk(const char * cs, const char * ct);
function_space_specifier char * Gstrtok(char * s, const char * ct);
function_space_specifier char * Gstrsep(char **s, const char *ct);
function_space_specifier char *strswab(const char *s);
function_space_specifier void * Gmemset(void * s, int c, size_t count);
function_space_specifier void * Gmemcpy(void *dest, const void *src, size_t count);
function_space_specifier void * Gmemmove(void * dest, const void *src, size_t count);
function_space_specifier int Gmemcmp(const void * cs, const void * ct, size_t count);
function_space_specifier void * Gmemscan(void * addr, int c, size_t size);
function_space_specifier char * Gstrstr(const char * s1, const char * s2);
function_space_specifier void * Gmemchr(const void *s, int c, size_t n);
function_space_specifier void Gstoupper(char *s);
function_space_specifier void Gstolower(char *s);
function_space_specifier char Gtoupper(char c);
function_space_specifier char Gtolower(char c);

