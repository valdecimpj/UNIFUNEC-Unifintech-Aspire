<?php

it('returns a credit score for a valid cpf', function () {
    $response = $this->getJson('/api/credit-scores?cpf=52998224725');

    $response
        ->assertSuccessful()
        ->assertJsonPath('cpf', '52998224725')
        ->assertJsonStructure(['cpf', 'score']);

    expect($response->json('score'))->toBeInt()->toBeBetween(0, 1000);
});

it('accepts a formatted cpf', function () {
    $this->getJson('/api/credit-scores?cpf=529.982.247-25')
        ->assertSuccessful()
        ->assertJsonPath('cpf', '52998224725');
});

it('rejects an invalid cpf', function (?string $cpf) {
    $this->getJson('/api/credit-scores?'.http_build_query(['cpf' => $cpf]))
        ->assertUnprocessable()
        ->assertJsonValidationErrors(['cpf']);
})->with([
    'missing' => [null],
    'empty' => [''],
    'too short' => ['123'],
    'repeated digits' => ['11111111111'],
    'invalid check digits' => ['52998224726'],
]);
