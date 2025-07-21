#pragma GCC optimize("O3,unroll-loops")

#include <iostream>
#include <vector>
#include <string>
#include <cstdint>
#include <algorithm>
#include <iomanip>
#include <sstream>
#include <numeric>
#include <stdexcept>
#include <chrono>
#include <bitset>
#include <functional>
#include <future>
#include <immintrin.h>

class Polynomial;
[[nodiscard]] inline Polynomial operator*(const Polynomial& a, const Polynomial& b);
[[nodiscard]] inline Polynomial operator/(const Polynomial& a, const Polynomial& b);
[[nodiscard]] inline Polynomial operator%(const Polynomial& a, const Polynomial& b);
[[nodiscard]] inline Polynomial multiply_karatsuba(const Polynomial& a, const Polynomial& b);

class Polynomial {
public:
    std::vector<uint64_t> words;

    Polynomial() = default;
    explicit Polynomial(std::vector<uint64_t> w) : words(std::move(w)) { trim(); }

    int degree() const {
        if (words.empty()) return -1;
        return (words.size() - 1) * 64 + (63 - __builtin_clzll(words.back()));
    }

    bool get_coeff(int i) const {
        if (i < 0) return false;
        size_t word_idx = i / 64;
        int bit_idx = i % 64;
        if (word_idx >= words.size()) return false;
        return (words[word_idx] >> bit_idx) & 1ULL;
    }

    void set_coeff(int i, bool val) {
        if (i < 0) return;
        size_t word_idx = i / 64;
        if (word_idx >= words.size()) words.resize(word_idx + 1, 0);
        if (val) words[word_idx] |= (1ULL << (i % 64));
        else words[word_idx] &= ~(1ULL << (i % 64));
    }
    
    void trim() {
        while (!words.empty() && words.back() == 0) words.pop_back();
    }
    
    Polynomial& operator+=(const Polynomial& rhs) {
        if (rhs.words.size() > words.size()) words.resize(rhs.words.size(), 0);
        for (size_t i = 0; i < rhs.words.size(); ++i) words[i] ^= rhs.words[i];
        trim();
        return *this;
    }
};

[[nodiscard]] inline Polynomial operator+(Polynomial lhs, const Polynomial& rhs) { lhs += rhs; return lhs; }
inline bool operator==(const Polynomial& a, const Polynomial& b) { return a.words == b.words; }
inline bool operator!=(const Polynomial& a, const Polynomial& b) { return !(a == b); }

[[nodiscard]] inline Polynomial shift(const Polynomial& p, int amount) {
    if (amount == 0 || p.words.empty()) return p;
    int word_shift = amount / 64;
    int bit_shift = amount % 64;
    Polynomial res;
    res.words.resize(p.words.size() + word_shift + (bit_shift != 0), 0);
    if (bit_shift == 0) {
        for (size_t i = 0; i < p.words.size(); ++i) res.words[i + word_shift] = p.words[i];
    } else {
        for (size_t i = 0; i < p.words.size(); ++i) {
            res.words[i + word_shift] ^= (p.words[i] << bit_shift);
            if (i + word_shift + 1 < res.words.size()) {
                res.words[i + word_shift + 1] ^= (p.words[i] >> (64 - bit_shift));
            }
        }
    }
    res.trim();
    return res;
}

Polynomial (*multiply_schoolbook_ptr)(const Polynomial&, const Polynomial&);

[[nodiscard]] inline Polynomial schoolbook_multiply_cpp(const Polynomial& a, const Polynomial& b) {
    if (a.words.empty() || b.words.empty()) return {};
    Polynomial res;
    const auto& small = (a.degree() < b.degree()) ? a : b;
    const auto& large = (a.degree() < b.degree()) ? b : a;
    for (size_t i = 0; i < small.words.size(); ++i) {
        uint64_t word = small.words[i];
        while (word != 0) {
            int bit_pos = __builtin_ctzll(word);
            res += shift(large, i * 64 + bit_pos);
            word &= word - 1;
        }
    }
    return res;
}

__attribute__((target("pclmul")))
[[nodiscard]] inline Polynomial schoolbook_multiply_pclmul(const Polynomial& a, const Polynomial& b) {
    if (a.words.empty() || b.words.empty()) return {};
    Polynomial res;
    res.words.resize(a.words.size() + b.words.size());
    for (size_t i = 0; i < a.words.size(); ++i) {
        __m128i a_word = _mm_set_epi64x(0, a.words[i]);
        for (size_t j = 0; j < b.words.size(); ++j) {
            __m128i b_word = _mm_set_epi64x(0, b.words[j]);
            __m128i prod = _mm_clmulepi64_si128(a_word, b_word, 0x00);
            res.words[i + j] ^= _mm_cvtsi128_si64(prod);
            res.words[i + j + 1] ^= _mm_extract_epi64(prod, 1);
        }
    }
    res.trim();
    return res;
}

const int KARATSUBA_THRESHOLD = 8;
[[nodiscard]] inline Polynomial multiply_karatsuba(const Polynomial& a, const Polynomial& b) {
    if (a.words.empty() || b.words.empty()) return {};
    if (a.words.size() <= KARATSUBA_THRESHOLD || b.words.size() <= KARATSUBA_THRESHOLD) {
        return (*multiply_schoolbook_ptr)(a, b);
    }

    size_t m = std::min(a.words.size(), b.words.size()) / 2;
    if (m == 0) return (*multiply_schoolbook_ptr)(a, b);

    Polynomial a_lo(std::vector<uint64_t>(a.words.begin(), a.words.begin() + m));
    Polynomial a_hi(std::vector<uint64_t>(a.words.begin() + m, a.words.end()));
    Polynomial b_lo(std::vector<uint64_t>(b.words.begin(), b.words.begin() + m));
    Polynomial b_hi(std::vector<uint64_t>(b.words.begin() + m, b.words.end()));

    Polynomial z0 = multiply_karatsuba(a_lo, b_lo);
    Polynomial z2 = multiply_karatsuba(a_hi, b_hi);
    Polynomial z1 = multiply_karatsuba(a_lo + a_hi, b_lo + b_hi);

    Polynomial middle = z1 + z0 + z2;
    return z0 + shift(middle, m * 64) + shift(z2, 2 * m * 64);
}

[[nodiscard]] inline Polynomial operator*(const Polynomial& a, const Polynomial& b) {
    return multiply_karatsuba(a, b);
}

[[nodiscard]] inline std::pair<Polynomial, Polynomial> divide(const Polynomial& a, const Polynomial& b) {
    if (b.words.empty()) throw std::runtime_error("Division by zero polynomial");
    Polynomial q, r = a;
    int deg_r = r.degree(), deg_b = b.degree();
    if (deg_r < deg_b) return {{}, r};
    q.words.resize((deg_r - deg_b) / 64 + 1, 0);
    while (deg_r >= deg_b) {
        int shift_amount = deg_r - deg_b;
        q.set_coeff(shift_amount, 1);
        r += shift(b, shift_amount);
        deg_r = r.degree();
    }
    q.trim();
    return {q, r};
}

[[nodiscard]] inline Polynomial operator/(const Polynomial& a, const Polynomial& b) { return divide(a, b).first; }
[[nodiscard]] inline Polynomial operator%(const Polynomial& a, const Polynomial& b) { return divide(a, b).second; }

[[nodiscard]] inline Polynomial gcd(Polynomial a, Polynomial b) {
    while (!b.words.empty()) {
        a = a % b;
        std::swap(a, b);
    }
    return a;
}

class CentauriDecoder {
public:
    std::vector<std::string> solve(int size_val, const std::string& line) {
        S = size_val;
        Polynomial B = hex_to_poly(line, 2 * S);
        std::vector<Polynomial> factors = factorize(B);
        return find_combinations(B, factors);
    }

private:
    int S;
    static const int MAX_PARALLEL_DEPTH = 3; 

    template <size_t MAX_DEG>
    std::vector<std::bitset<MAX_DEG>> build_berlekamp_matrix(const Polynomial& f) {
        int n = f.degree();
        std::vector<std::bitset<MAX_DEG>> M(n);
        Polynomial p; p.set_coeff(0, 1);
        for (int i = 0; i < n; ++i) {
            Polynomial p_i = p;
            p_i.set_coeff(i, !p_i.get_coeff(i));
            for (int j = 0; j < n; ++j) {
                if (p_i.get_coeff(j)) M[i][j] = 1;
            }
            if (i + 1 < n) p = shift(p, 2) % f;
        }
        std::vector<std::bitset<MAX_DEG>> MT(n);
        for(int i=0; i<n; ++i) for(int j=0; j<n; ++j) if(M[j][i]) MT[i][j] = 1;
        return MT;
    }

    template <size_t MAX_DEG>
    std::vector<Polynomial> find_nullspace(const std::vector<std::bitset<MAX_DEG>>& M_T, int n) {
        auto A = M_T;
        std::vector<int> pivot_col(n, -1);
        int rank = 0;
        for (int col = 0; col < n && rank < n; ++col) {
            int pivot_row = rank;
            while (pivot_row < n && !A[pivot_row].test(col)) pivot_row++;
            if (pivot_row < n) {
                std::swap(A[rank], A[pivot_row]);
                for (int i = 0; i < n; ++i) if (i != rank && A[i].test(col)) A[i] ^= A[rank];
                pivot_col[rank++] = col;
            }
        }
        std::vector<Polynomial> basis;
        std::vector<bool> is_free(n, true);
        for(int i = 0; i < rank; ++i) if(pivot_col[i] !=-1) is_free[pivot_col[i]] = false;
        for (int j = 0; j < n; ++j) {
            if (is_free[j]) {
                Polynomial sol;
                sol.set_coeff(j, 1);
                for (int i = 0; i < rank; ++i) if (A[i].test(j)) sol.set_coeff(pivot_col[i], 1);
                sol.trim();
                basis.push_back(sol);
            }
        }
        return basis;
    }

    std::vector<Polynomial> get_nullspace_basis(const Polynomial& f) {
        int d = f.degree();
        if (d <= 0) return {};
        if (d <= 64)   return find_nullspace(build_berlekamp_matrix<64>(f), d);
        if (d <= 128)  return find_nullspace(build_berlekamp_matrix<128>(f), d);
        if (d <= 256)  return find_nullspace(build_berlekamp_matrix<256>(f), d);
        if (d <= 512)  return find_nullspace(build_berlekamp_matrix<512>(f), d);
        if (d <= 1024) return find_nullspace(build_berlekamp_matrix<1024>(f), d);
        if (d <= 2048) return find_nullspace(build_berlekamp_matrix<2048>(f), d);
        throw std::runtime_error("Degree " + std::to_string(d) + " too large for pre-compiled bitset sizes.");
    }
    
    std::vector<Polynomial> factorize(Polynomial f) {
        return factorize_recursive(std::move(f), 0);
    }
    
    std::vector<Polynomial> factorize_recursive(Polynomial p, int depth) {
        if (p.degree() <= 0) return {};
        auto basis = get_nullspace_basis(p);
        if (basis.size() <= 1) return {p};

        Polynomial h;
        for(const auto& b : basis) if(b.degree() > 0) { h = b; break; }

        Polynomial g = gcd(p, h);
        Polynomial p_div_g = p / g;

        std::vector<Polynomial> f1, f2;
        if (depth < MAX_PARALLEL_DEPTH) {
            auto future1 = std::async(std::launch::async, &CentauriDecoder::factorize_recursive, this, g, depth + 1);
            auto future2 = std::async(std::launch::async, &CentauriDecoder::factorize_recursive, this, p_div_g, depth + 1);
            f1 = future1.get();
            f2 = future2.get();
        } else {
            f1 = factorize_recursive(g, depth + 1);
            f2 = factorize_recursive(p_div_g, depth + 1);
        }

        f1.insert(f1.end(), f2.begin(), f2.end());
        return f1;
    }

    void combine_recursive(const Polynomial& B, const std::vector<Polynomial>& factors, int k, Polynomial current_X, std::vector<std::pair<std::string, std::string>>& pairs) {
        if (k == (int)factors.size()) {
            if (current_X.degree() < S) {
                Polynomial current_Y = B / current_X;
                if (current_Y.degree() < S) {
                    pairs.push_back({poly_to_hex(current_X, S), poly_to_hex(current_Y, S)});
                }
            }
            return;
        }

        combine_recursive(B, factors, k + 1, current_X, pairs);
        if (current_X.degree() + factors[k].degree() < S) {
            Polynomial next_X = current_X * factors[k];
            combine_recursive(B, factors, k + 1, next_X, pairs);
        }
    }

    std::vector<std::string> find_combinations(const Polynomial& B, const std::vector<Polynomial>& factors) {
        std::vector<std::pair<std::string, std::string>> pairs;
        Polynomial one; one.set_coeff(0, 1);
        combine_recursive(B, factors, 0, one, pairs);

        std::vector<std::string> final_candidates;
        for (const auto& p : pairs) {
            final_candidates.push_back(p.first + " " + p.second);
            if (p.first != p.second) {
                final_candidates.push_back(p.second + " " + p.first);
            }
        }

        std::sort(final_candidates.begin(), final_candidates.end());
        final_candidates.erase(std::unique(final_candidates.begin(), final_candidates.end()), final_candidates.end());
        return final_candidates;
    }

    Polynomial hex_to_poly(const std::string& hex_line, int total_bits) {
        std::stringstream ss(hex_line);
        int num_words32 = total_bits / 32;
        std::vector<uint32_t> arr32(num_words32);
        for (int i = 0; i < num_words32; ++i) ss >> std::hex >> arr32[i];
        Polynomial p;
        p.words.resize((total_bits + 63) / 64, 0);
        for (int i = 0; i < num_words32; ++i) p.words[i / 2] |= ((uint64_t)arr32[i] << (32 * (i % 2)));
        p.trim();
        return p;
    }

    std::string poly_to_hex(const Polynomial& p, int total_bits) {
        int num_words32 = total_bits / 32;
        std::vector<uint32_t> arr32(num_words32, 0);
        for (int i = 0; i < total_bits; ++i) if (p.get_coeff(i)) arr32[i / 32] |= (1u << (i % 32));
        std::ostringstream oss;
        for (size_t i = 0; i < arr32.size(); ++i) {
            oss << std::setw(8) << std::setfill('0') << std::hex << arr32[i] << (i < arr32.size() - 1 ? " " : "");
        }
        return oss.str();
    }
};

void init_cpu_features() {
    if (__builtin_cpu_supports("pclmul")) {
        multiply_schoolbook_ptr = &schoolbook_multiply_pclmul;
        std::cerr << "[DEBUG] PCLMULQDQ hardware acceleration enabled." << std::endl;
    } else {
        multiply_schoolbook_ptr = &schoolbook_multiply_cpp;
        std::cerr << "[DEBUG] PCLMULQDQ not supported. Using fallback C++ implementation." << std::endl;
    }
}

int main() {
    std::ios_base::sync_with_stdio(false);
    std::cin.tie(NULL);
    
    init_cpu_features();

    int s;
    std::cin >> s; std::cin.ignore();
    std::string line;
    std::getline(std::cin, line);
    
    auto total_start = std::chrono::steady_clock::now();
    
    CentauriDecoder decoder;
    std::vector<std::string> answers = decoder.solve(s, line);

    auto total_end = std::chrono::steady_clock::now();
    
    std::cerr << "[DEBUG] Total execution time: " 
              << std::chrono::duration_cast<std::chrono::microseconds>(total_end - total_start).count() 
              << " us\n";

    for (const auto& ans : answers) {
        std::cout << ans << "\n";
    }

    return 0;
}