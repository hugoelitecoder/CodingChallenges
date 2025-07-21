cat > ./A << EOF
#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <gmp.h>
#include <thread>
#include <functional>

using namespace std;

// RAII wrapper for mpz_t to automate memory management (init/clear).
class GmpInt {
public:
    mpz_t v;
    GmpInt() { mpz_init(v); }
    ~GmpInt() { mpz_clear(v); }
    GmpInt(const GmpInt& other) { mpz_init_set(v, other.v); }
    GmpInt& operator=(const GmpInt& other) {
        if (this != &other) { mpz_set(v, other.v); }
        return *this;
    }
};

class PiCalculator {
public:
    string getDigits(int index, int n);

private:
    static const unsigned long long A = 13591409ULL;
    static const unsigned long long B = 545140134ULL;
    static const unsigned long long C = 640320ULL;
    static const unsigned long long D = 12ULL;
    static const unsigned long long E = 426880ULL;
    static const unsigned long long F = 10005ULL;
    static const unsigned long long G = 10939058860032000ULL; // C*C*C/24

    void binary_split(GmpInt& P, GmpInt& Q, GmpInt& T, long a, long b, int depth, int max_depth);
};

void PiCalculator::binary_split(GmpInt& P, GmpInt& Q, GmpInt& T, long a, long b, int depth, int max_depth) {
    if (b == a + 1) {
        if (a == 0) {
            mpz_set_ui(P.v, 1);
            mpz_set_ui(Q.v, 1);
        } else {
            mpz_set_si(P.v, 6 * a - 5);
            mpz_mul_si(P.v, P.v, 2 * a - 1);
            mpz_mul_si(P.v, P.v, 6 * a - 1);
            mpz_set_ui(Q.v, a);
            mpz_pow_ui(Q.v, Q.v, 3);
            mpz_mul_ui(Q.v, Q.v, G);
        }
        mpz_set_ui(T.v, a);
        mpz_mul_ui(T.v, T.v, B);
        mpz_add_ui(T.v, T.v, A);
        mpz_mul(T.v, T.v, P.v);
        if ((a % 2) == 1) {
            mpz_neg(T.v, T.v);
        }
        return;
    }

    long m = (a + b) / 2;
    GmpInt P1, Q1, T1, P2, Q2, T2;

    if (depth < max_depth) {
        thread t1(&PiCalculator::binary_split, this, std::ref(P1), std::ref(Q1), std::ref(T1), a, m, depth + 1, max_depth);
        this->binary_split(P2, Q2, T2, m, b, depth + 1, max_depth);
        t1.join();
    } else {
        this->binary_split(P1, Q1, T1, a, m, depth + 1, max_depth);
        this->binary_split(P2, Q2, T2, m, b, depth + 1, max_depth);
    }
    
    mpz_mul(P.v, P1.v, P2.v);
    mpz_mul(Q.v, Q1.v, Q2.v);
    mpz_mul(T1.v, T1.v, Q2.v);
    mpz_mul(P1.v, P1.v, T2.v);
    mpz_add(T.v, T1.v, P1.v);
}

string PiCalculator::getDigits(int index, int n) {
    unsigned int num_cores = std::thread::hardware_concurrency();
    int max_parallel_depth = 0;
    if (num_cores > 1) {
        unsigned int threads = 1;
        while (threads < num_cores) {
            threads <<= 1;
            max_parallel_depth++;
        }
    }

    const int final_digits_needed = 1000000 + 50 + 10;
    const int scaling_digits = final_digits_needed * 2;
    const int num_terms = final_digits_needed / 14 + 2;

    GmpInt P, Q, T, scale_factor, sqrt_arg, sqrt_val, pi_val;
    binary_split(P, Q, T, 0, num_terms, 0, max_parallel_depth);
    
    mpz_ui_pow_ui(scale_factor.v, 10, scaling_digits);
    mpz_mul_ui(sqrt_arg.v, scale_factor.v, F);
    mpz_sqrt(sqrt_val.v, sqrt_arg.v);
    mpz_mul(pi_val.v, Q.v, sqrt_val.v);
    mpz_mul_ui(pi_val.v, pi_val.v, E);
    mpz_fdiv_q(pi_val.v, pi_val.v, T.v);

    char* pi_c_str = mpz_get_str(NULL, 10, pi_val.v);
    string pi_str(pi_c_str);
    free(pi_c_str);
    
    return pi_str.substr(index, n);
}

int main()
{
    ios_base::sync_with_stdio(false);
    cin.tie(NULL);
    
    int index;
    cin >> index;
    int n;
    cin >> n;

    PiCalculator calculator;
    cout << calculator.getDigits(index, n) << endl;

    return 0;
}
EOF
g++ -o p -x c++ A -O3 -march=native -flto -lgmp -lpthread
./p