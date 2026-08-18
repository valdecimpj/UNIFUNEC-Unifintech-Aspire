<?php

use App\Http\Controllers\CreditScoreController;
use Illuminate\Support\Facades\Route;

Route::get('/credit-scores', [CreditScoreController::class, 'show']);
