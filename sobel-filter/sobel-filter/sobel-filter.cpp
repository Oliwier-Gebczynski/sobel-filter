extern "C" int _fastcall MyProc1(long long x, long long y);

int main(int argc, char* argv[])
{
	int x = 3, y = 4, z = 0;

	z = MyProc1(x, y);		// wywołanie procedury asemblerowej z biblioteki

	return 0;
}
