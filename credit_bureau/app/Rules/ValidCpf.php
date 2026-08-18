<?php

namespace App\Rules;

use Closure;
use Illuminate\Contracts\Validation\ValidationRule;

class ValidCpf implements ValidationRule
{
    public function validate(string $attribute, mixed $value, Closure $fail): void
    {
        if (! is_string($value) && ! is_numeric($value)) {
            $fail('The :attribute must be a valid CPF.');

            return;
        }

        $cpf = preg_replace('/\D+/', '', (string) $value) ?? '';

        if (! $this->isValid($cpf)) {
            $fail('The :attribute must be a valid CPF.');
        }
    }

    private function isValid(string $cpf): bool
    {
        if (strlen($cpf) !== 11) {
            return false;
        }

        if (preg_match('/^(\d)\1{10}$/', $cpf) === 1) {
            return false;
        }

        return $this->checkDigit($cpf, 9) && $this->checkDigit($cpf, 10);
    }

    private function checkDigit(string $cpf, int $position): bool
    {
        $sum = 0;

        for ($index = 0; $index < $position; $index++) {
            $sum += (int) $cpf[$index] * (($position + 1) - $index);
        }

        $remainder = $sum % 11;
        $expectedDigit = $remainder < 2 ? 0 : 11 - $remainder;

        return (int) $cpf[$position] === $expectedDigit;
    }
}
