<?php

namespace App\Http\Controllers;

use App\Http\Requests\ConsultCreditScoreRequest;
use Illuminate\Http\JsonResponse;

class CreditScoreController extends Controller
{
    public function show(ConsultCreditScoreRequest $request): JsonResponse
    {
        return response()->json([
            'cpf' => $request->validated('cpf'),
            'score' => random_int(0, 1000),
        ]);
    }
}
