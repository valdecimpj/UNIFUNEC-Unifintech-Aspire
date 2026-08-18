<?php

namespace App\Http\Requests;

use App\Rules\ValidCpf;
use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Support\Str;

class ConsultCreditScoreRequest extends FormRequest
{
    protected function prepareForValidation(): void
    {
        if (! $this->has('cpf')) {
            return;
        }

        $this->merge([
            'cpf' => Str::of((string) $this->cpf)->replaceMatches('/\D+/', '')->toString(),
        ]);
    }

    /**
     * @return array<string, list<mixed>>
     */
    public function rules(): array
    {
        return [
            'cpf' => ['required', 'string', 'size:11', new ValidCpf],
        ];
    }
}
